using Microsoft.EntityFrameworkCore;
using TimberHaul.API.Data;
using TimberHaul.API.DTOs;
using TimberHaul.API.Models;

namespace TimberHaul.API.Services;

public interface IPaymentService
{
    Task<ApiResponse<PaymentResponseDto>> CreatePaymentAsync(Guid foresterId, CreatePaymentDto dto);
    Task<ApiResponse<PaymentResponseDto>> RecordPaymentAsync(Guid paymentId, Guid customerId, RecordPaymentDto dto);
    Task<ApiResponse<List<PaymentListDto>>> GetCustomerPaymentsAsync(Guid customerId);
    Task<ApiResponse<List<PaymentListDto>>> GetForesterPaymentsAsync(Guid foresterId);
    Task<ApiResponse<PaymentResponseDto>> GetPaymentByIdAsync(Guid paymentId, Guid userId);
    Task<ApiResponse<PaymentSummaryDto>> GetPaymentSummaryAsync(Guid userId, string role);
    Task<ApiResponse<PaymentResponseDto>> MarkPaymentAsPaidAsync(Guid paymentId, Guid customerId);
}

public class PaymentService : IPaymentService
{
    private readonly TimberHaulDbContext _context;

    public PaymentService(TimberHaulDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PaymentResponseDto>> CreatePaymentAsync(Guid foresterId, CreatePaymentDto dto)
    {
        var load = await _context.Loads
            .Include(l => l.Customer)
                .ThenInclude(c => c!.User)
            .FirstOrDefaultAsync(l => l.LoadId == dto.LoadId);

        if (load == null)
        {
            return new ApiResponse<PaymentResponseDto>
            {
                Success = false,
                Message = "Load not found"
            };
        }

        if (load.ForesterId != foresterId)
        {
            return new ApiResponse<PaymentResponseDto>
            {
                Success = false,
                Message = "This is not your load"
            };
        }

        var existingPayment = await _context.Payments
            .FirstOrDefaultAsync(p => p.LoadId == dto.LoadId);

        if (existingPayment != null)
        {
            return new ApiResponse<PaymentResponseDto>
            {
                Success = false,
                Message = "Payment already exists for this load"
            };
        }

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        var payment = new Payment
        {
            LoadId = dto.LoadId,
            CustomerId = load.CustomerId,
            ForesterId = foresterId,
            Amount = load.TotalAmount,
            DueDate = dto.DueDate,
            Status = PaymentStatus.Unpaid,
            InvoiceNumber = invoiceNumber,
            Notes = dto.Notes
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var forester = await _context.ForesterProfiles
            .Include(f => f.User)
            .FirstAsync(f => f.ForesterId == foresterId);

        var paymentDto = new PaymentResponseDto
        {
            PaymentId = payment.PaymentId,
            LoadId = payment.LoadId,
            LoadNumber = load.LoadNumber,
            CustomerId = load.CustomerId,
            CustomerName = load.Customer != null ? load.Customer.User.FirstName + " " + load.Customer.User.LastName : "",
            ForesterId = foresterId,
            ForesterName = forester.User.FirstName + " " + forester.User.LastName,
            Amount = payment.Amount,
            PaymentMethodType = payment.PaymentMethodType,
            PaymentDate = payment.PaymentDate,
            DueDate = payment.DueDate,
            Status = payment.Status,
            InvoiceNumber = payment.InvoiceNumber,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        };

        return new ApiResponse<PaymentResponseDto>
        {
            Success = true,
            Message = "Payment invoice created successfully",
            Data = paymentDto
        };
    }

    public async Task<ApiResponse<PaymentResponseDto>> RecordPaymentAsync(Guid paymentId, Guid customerId, RecordPaymentDto dto)
    {
        var payment = await _context.Payments
            .Include(p => p.Load)
            .Include(p => p.Customer)
                .ThenInclude(c => c!.User)
            .Include(p => p.Forester)
                .ThenInclude(f => f!.User)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null)
        {
            return new ApiResponse<PaymentResponseDto>
            {
                Success = false,
                Message = "Payment not found"
            };
        }

        if (payment.CustomerId != customerId)
        {
            return new ApiResponse<PaymentResponseDto>
            {
                Success = false,
                Message = "This is not your payment"
            };
        }

        if (payment.Status == PaymentStatus.Paid)
        {
            return new ApiResponse<PaymentResponseDto>
            {
                Success = false,
                Message = "Payment already marked as paid"
            };
        }

        payment.Status = PaymentStatus.Paid;
        payment.PaymentMethodType = dto.PaymentMethod;
        payment.PaymentDate = DateTime.UtcNow;
        if (dto.Notes != null) payment.Notes = dto.Notes;
        payment.UpdatedAt = DateTime.UtcNow;

        if (payment.Load != null)
        {
            payment.Load.PaymentStatus = PaymentStatus.Paid;
            payment.Load.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var paymentDto = new PaymentResponseDto
        {
            PaymentId = payment.PaymentId,
            LoadId = payment.LoadId,
            LoadNumber = payment.Load?.LoadNumber,
            CustomerId = payment.CustomerId,
            CustomerName = payment.Customer != null ? payment.Customer.User.FirstName + " " + payment.Customer.User.LastName : "",
            ForesterId = payment.ForesterId,
            ForesterName = payment.Forester != null ? payment.Forester.User.FirstName + " " + payment.Forester.User.LastName : "",
            Amount = payment.Amount,
            PaymentMethodType = payment.PaymentMethodType,
            PaymentDate = payment.PaymentDate,
            DueDate = payment.DueDate,
            Status = payment.Status,
            InvoiceNumber = payment.InvoiceNumber,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        };

        return new ApiResponse<PaymentResponseDto>
        {
            Success = true,
            Message = "Payment recorded successfully",
            Data = paymentDto
        };
    }

    public async Task<ApiResponse<List<PaymentListDto>>> GetCustomerPaymentsAsync(Guid customerId)
    {
        var payments = await _context.Payments
            .Include(p => p.Load)
            .Include(p => p.Customer)
                .ThenInclude(c => c!.User)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentListDto
            {
                PaymentId = p.PaymentId,
                LoadNumber = p.Load != null ? p.Load.LoadNumber : null,
                CustomerName = p.Customer != null ? p.Customer.User.FirstName + " " + p.Customer.User.LastName : "",
                Amount = p.Amount,
                DueDate = p.DueDate,
                Status = p.Status,
                InvoiceNumber = p.InvoiceNumber
            })
            .ToListAsync();

        return new ApiResponse<List<PaymentListDto>>
        {
            Success = true,
            Message = "Payments retrieved successfully",
            Data = payments
        };
    }

    public async Task<ApiResponse<List<PaymentListDto>>> GetForesterPaymentsAsync(Guid foresterId)
    {
        var payments = await _context.Payments
            .Include(p => p.Load)
            .Include(p => p.Customer)
                .ThenInclude(c => c!.User)
            .Where(p => p.ForesterId == foresterId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentListDto
            {
                PaymentId = p.PaymentId,
                LoadNumber = p.Load != null ? p.Load.LoadNumber : null,
                CustomerName = p.Customer != null ? p.Customer.User.FirstName + " " + p.Customer.User.LastName : "",
                Amount = p.Amount,
                DueDate = p.DueDate,
                Status = p.Status,
                InvoiceNumber = p.InvoiceNumber
            })
            .ToListAsync();

        return new ApiResponse<List<PaymentListDto>>
        {
            Success = true,
            Message = "Payments retrieved successfully",
            Data = payments
        };
    }

    public async Task<ApiResponse<PaymentResponseDto>> GetPaymentByIdAsync(Guid paymentId, Guid userId)
    {
        var payment = await _context.Payments
            .Include(p => p.Load)
            .Include(p => p.Customer)
                .ThenInclude(c => c!.User)
            .Include(p => p.Forester)
                .ThenInclude(f => f!.User)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null)
        {
            return new ApiResponse<PaymentResponseDto>
            {
                Success = false,
                Message = "Payment not found"
            };
        }

        if (payment.CustomerId != userId && payment.ForesterId != userId)
        {
            return new ApiResponse<PaymentResponseDto>
            {
                Success = false,
                Message = "You don't have access to this payment"
            };
        }

        var paymentDto = new PaymentResponseDto
        {
            PaymentId = payment.PaymentId,
            LoadId = payment.LoadId,
            LoadNumber = payment.Load?.LoadNumber,
            CustomerId = payment.CustomerId,
            CustomerName = payment.Customer != null ? payment.Customer.User.FirstName + " " + payment.Customer.User.LastName : "",
            ForesterId = payment.ForesterId,
            ForesterName = payment.Forester != null ? payment.Forester.User.FirstName + " " + payment.Forester.User.LastName : "",
            Amount = payment.Amount,
            PaymentMethodType = payment.PaymentMethodType,
            PaymentDate = payment.PaymentDate,
            DueDate = payment.DueDate,
            Status = payment.Status,
            InvoiceNumber = payment.InvoiceNumber,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        };

        return new ApiResponse<PaymentResponseDto>
        {
            Success = true,
            Message = "Payment retrieved successfully",
            Data = paymentDto
        };
    }

    public async Task<ApiResponse<PaymentSummaryDto>> GetPaymentSummaryAsync(Guid userId, string role)
    {
        IQueryable<Payment> query = _context.Payments;

        if (role == "Customer")
        {
            query = query.Where(p => p.CustomerId == userId);
        }
        else if (role == "Forester")
        {
            query = query.Where(p => p.ForesterId == userId);
        }

        var payments = await query.ToListAsync();
        var now = DateTime.UtcNow;

        var summary = new PaymentSummaryDto
        {
            TotalPayments = payments.Count,
            PaidCount = payments.Count(p => p.Status == PaymentStatus.Paid),
            UnpaidCount = payments.Count(p => p.Status == PaymentStatus.Unpaid && p.DueDate >= now),
            OverdueCount = payments.Count(p => p.Status == PaymentStatus.Unpaid && p.DueDate < now),
            TotalAmount = payments.Sum(p => p.Amount),
            PaidAmount = payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount),
            UnpaidAmount = payments.Where(p => p.Status == PaymentStatus.Unpaid && p.DueDate >= now).Sum(p => p.Amount),
            OverdueAmount = payments.Where(p => p.Status == PaymentStatus.Unpaid && p.DueDate < now).Sum(p => p.Amount)
        };

        return new ApiResponse<PaymentSummaryDto>
        {
            Success = true,
            Message = "Payment summary retrieved successfully",
            Data = summary
        };
    }

    public async Task<ApiResponse<PaymentResponseDto>> MarkPaymentAsPaidAsync(Guid paymentId, Guid customerId)
    {
        var payment = await _context.Payments
            .Include(p => p.Load)
            .Include(p => p.Customer)
                .ThenInclude(c => c!.User)
            .Include(p => p.Forester)
                .ThenInclude(f => f!.User)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null)
        {
            return new ApiResponse<PaymentResponseDto>
            {
                Success = false,
                Message = "Payment not found"
            };
        }

        if (payment.CustomerId != customerId)
        {
            return new ApiResponse<PaymentResponseDto>
            {
                Success = false,
                Message = "This is not your payment"
            };
        }

        if (payment.Status == PaymentStatus.Paid)
        {
            return new ApiResponse<PaymentResponseDto>
            {
                Success = false,
                Message = "Payment already marked as paid"
            };
        }

        payment.Status = PaymentStatus.Paid;
        payment.PaymentMethodType = PaymentMethod.Cash;
        payment.PaymentDate = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        if (payment.Load != null)
        {
            payment.Load.PaymentStatus = PaymentStatus.Paid;
            payment.Load.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var paymentDto = new PaymentResponseDto
        {
            PaymentId = payment.PaymentId,
            LoadId = payment.LoadId,
            LoadNumber = payment.Load?.LoadNumber,
            CustomerId = payment.CustomerId,
            CustomerName = payment.Customer != null ? payment.Customer.User.FirstName + " " + payment.Customer.User.LastName : "",
            ForesterId = payment.ForesterId,
            ForesterName = payment.Forester != null ? payment.Forester.User.FirstName + " " + payment.Forester.User.LastName : "",
            Amount = payment.Amount,
            PaymentMethodType = payment.PaymentMethodType,
            PaymentDate = payment.PaymentDate,
            DueDate = payment.DueDate,
            Status = payment.Status,
            InvoiceNumber = payment.InvoiceNumber,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        };

        return new ApiResponse<PaymentResponseDto>
        {
            Success = true,
            Message = "Payment marked as paid successfully",
            Data = paymentDto
        };
    }
}