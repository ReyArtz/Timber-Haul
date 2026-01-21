using Microsoft.EntityFrameworkCore;
using TimberHaul.API.Data;
using TimberHaul.API.DTOs;
using TimberHaul.API.Models;

namespace TimberHaul.API.Services;

public interface ILoadService
{
    Task<ApiResponse<LoadResponseDto>> CreateLoadAsync(Guid foresterId, CreateLoadDto dto);
    Task<ApiResponse<List<LoadListDto>>> GetDriverLoadsAsync(Guid driverId);
    Task<ApiResponse<List<LoadListDto>>> GetForesterLoadsAsync(Guid foresterId);
    Task<ApiResponse<LoadResponseDto>> GetLoadByIdAsync(Guid loadId, Guid userId);
    Task<ApiResponse<LoadResponseDto>> UpdateLoadStatusAsync(Guid loadId, Guid driverId, UpdateLoadStatusDto dto);
    Task<ApiResponse<LoadResponseDto>> UploadPhotoAsync(Guid loadId, Guid driverId, UploadPhotoDto dto);
}

public class LoadService : ILoadService
{
    private readonly TimberHaulDbContext _context;

    public LoadService(TimberHaulDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<LoadResponseDto>> CreateLoadAsync(Guid foresterId, CreateLoadDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId);

        if (order == null)
        {
            return new ApiResponse<LoadResponseDto>
            {
                Success = false,
                Message = "Order not found"
            };
        }

        if (order.ForesterId != foresterId)
        {
            return new ApiResponse<LoadResponseDto>
            {
                Success = false,
                Message = "This is not your order"
            };
        }

        if (order.OrderStatus != "confirmed")
        {
            return new ApiResponse<LoadResponseDto>
            {
                Success = false,
                Message = "Order must be confirmed before creating a load"
            };
        }

        var driverExists = await _context.DeliveryProfiles.AnyAsync(d => d.DriverId == dto.DriverId);
        if (!driverExists)
        {
            return new ApiResponse<LoadResponseDto>
            {
                Success = false,
                Message = "Driver not found"
            };
        }

        var loadNumber = $"LOAD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        var load = new Load
        {
            LoadNumber = loadNumber,
            OrderId = dto.OrderId,
            ForesterId = order.ForesterId,
            CustomerId = order.CustomerId,
            DriverId = dto.DriverId,
            PlotId = dto.PlotId,
            ProductId = order.ProductId,
            WoodType = order.Product?.WoodType ?? WoodType.Salcam,
            Volume = order.Volume,
            PricePerCubicMeter = order.PricePerUnit,
            TotalAmount = order.TotalAmount,
            DeliveryLocation = order.DeliveryAddress,
            Notes = dto.Notes,
            Status = LoadStatus.Pending,
            PaymentStatus = PaymentStatus.Unpaid
        };

        _context.Loads.Add(load);

        order.OrderStatus = "assigned_to_driver";
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var driver = await _context.DeliveryProfiles
            .Include(d => d.User)
            .FirstAsync(d => d.DriverId == dto.DriverId);

        var forester = await _context.ForesterProfiles
            .Include(f => f.User)
            .FirstAsync(f => f.ForesterId == foresterId);

        var loadDto = new LoadResponseDto
        {
            LoadId = load.LoadId,
            LoadNumber = load.LoadNumber,
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer != null ? order.Customer.User.FirstName + " " + order.Customer.User.LastName : "",
            DriverId = dto.DriverId,
            DriverName = driver.User.FirstName + " " + driver.User.LastName,
            ForesterId = foresterId,
            ForesterName = forester.User.FirstName + " " + forester.User.LastName,
            WoodType = load.WoodType,
            Volume = load.Volume,
            PricePerCubicMeter = load.PricePerCubicMeter,
            TotalAmount = load.TotalAmount,
            DeliveryLocation = load.DeliveryLocation,
            Notes = load.Notes,
            Status = load.Status,
            PaymentStatus = load.PaymentStatus,
            CreatedAt = load.CreatedAt
        };

        return new ApiResponse<LoadResponseDto>
        {
            Success = true,
            Message = "Load created successfully",
            Data = loadDto
        };
    }

    public async Task<ApiResponse<List<LoadListDto>>> GetDriverLoadsAsync(Guid driverId)
    {
        var loads = await _context.Loads
            .Include(l => l.Customer)
                .ThenInclude(c => c!.User)
            .Where(l => l.DriverId == driverId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LoadListDto
            {
                LoadId = l.LoadId,
                LoadNumber = l.LoadNumber,
                CustomerName = l.Customer != null ? l.Customer.User.FirstName + " " + l.Customer.User.LastName : "",
                Volume = l.Volume,
                DeliveryLocation = l.DeliveryLocation,
                Status = l.Status,
                PaymentStatus = l.PaymentStatus,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new ApiResponse<List<LoadListDto>>
        {
            Success = true,
            Message = "Loads retrieved successfully",
            Data = loads
        };
    }

    public async Task<ApiResponse<List<LoadListDto>>> GetForesterLoadsAsync(Guid foresterId)
    {
        var loads = await _context.Loads
            .Include(l => l.Customer)
                .ThenInclude(c => c!.User)
            .Where(l => l.ForesterId == foresterId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LoadListDto
            {
                LoadId = l.LoadId,
                LoadNumber = l.LoadNumber,
                CustomerName = l.Customer != null ? l.Customer.User.FirstName + " " + l.Customer.User.LastName : "",
                Volume = l.Volume,
                DeliveryLocation = l.DeliveryLocation,
                Status = l.Status,
                PaymentStatus = l.PaymentStatus,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new ApiResponse<List<LoadListDto>>
        {
            Success = true,
            Message = "Loads retrieved successfully",
            Data = loads
        };
    }

    public async Task<ApiResponse<LoadResponseDto>> GetLoadByIdAsync(Guid loadId, Guid userId)
    {
        var load = await _context.Loads
            .Include(l => l.Order)
            .Include(l => l.Customer)
                .ThenInclude(c => c!.User)
            .Include(l => l.Driver)
                .ThenInclude(d => d!.User)
            .Include(l => l.Forester)
                .ThenInclude(f => f!.User)
            .Include(l => l.Plot)
            .FirstOrDefaultAsync(l => l.LoadId == loadId);

        if (load == null)
        {
            return new ApiResponse<LoadResponseDto>
            {
                Success = false,
                Message = "Load not found"
            };
        }

        if (load.CustomerId != userId && load.DriverId != userId && load.ForesterId != userId)
        {
            return new ApiResponse<LoadResponseDto>
            {
                Success = false,
                Message = "You don't have access to this load"
            };
        }

        var loadDto = new LoadResponseDto
        {
            LoadId = load.LoadId,
            LoadNumber = load.LoadNumber,
            OrderId = load.OrderId,
            OrderNumber = load.Order?.OrderNumber,
            CustomerId = load.CustomerId,
            CustomerName = load.Customer != null ? load.Customer.User.FirstName + " " + load.Customer.User.LastName : "",
            DriverId = load.DriverId,
            DriverName = load.Driver != null ? load.Driver.User.FirstName + " " + load.Driver.User.LastName : "",
            ForesterId = load.ForesterId,
            ForesterName = load.Forester != null ? load.Forester.User.FirstName + " " + load.Forester.User.LastName : "",
            PlotName = load.Plot?.PlotName,
            WoodType = load.WoodType,
            Volume = load.Volume,
            PricePerCubicMeter = load.PricePerCubicMeter,
            TotalAmount = load.TotalAmount,
            DeliveryLocation = load.DeliveryLocation,
            Notes = load.Notes,
            Status = load.Status,
            PaymentStatus = load.PaymentStatus,
            BeforeLoadPhoto = load.BeforeLoadPhoto,
            OnTruckPhoto = load.OnTruckPhoto,
            DeliveredPhoto = load.DeliveredPhoto,
            CreatedAt = load.CreatedAt,
            LoadedAt = load.LoadedAt,
            DeliveredAt = load.DeliveredAt
        };

        return new ApiResponse<LoadResponseDto>
        {
            Success = true,
            Message = "Load retrieved successfully",
            Data = loadDto
        };
    }

    public async Task<ApiResponse<LoadResponseDto>> UpdateLoadStatusAsync(Guid loadId, Guid driverId, UpdateLoadStatusDto dto)
    {
        var load = await _context.Loads
            .Include(l => l.Order)
            .Include(l => l.Customer)
                .ThenInclude(c => c!.User)
            .Include(l => l.Driver)
                .ThenInclude(d => d!.User)
            .Include(l => l.Forester)
                .ThenInclude(f => f!.User)
            .FirstOrDefaultAsync(l => l.LoadId == loadId);

        if (load == null)
        {
            return new ApiResponse<LoadResponseDto>
            {
                Success = false,
                Message = "Load not found"
            };
        }

        if (load.DriverId != driverId)
        {
            return new ApiResponse<LoadResponseDto>
            {
                Success = false,
                Message = "You can only update your own loads"
            };
        }

        load.Status = dto.Status;
        if (dto.Notes != null) load.Notes = dto.Notes;

        if (dto.Status == LoadStatus.OnTruck && load.LoadedAt == null)
        {
            load.LoadedAt = DateTime.UtcNow;
        }

        if (dto.Status == LoadStatus.Delivered && load.DeliveredAt == null)
{
    load.DeliveredAt = DateTime.UtcNow;
    if (load.Order != null)
    {
        load.Order.OrderStatus = "completed";
        load.Order.UpdatedAt = DateTime.UtcNow;
    }

    // Auto-create payment/invoice when delivered
    var existingPayment = await _context.Payments
        .FirstOrDefaultAsync(p => p.LoadId == loadId);

    if (existingPayment == null)
    {
        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        var payment = new Payment
        {
            LoadId = loadId,
            CustomerId = load.CustomerId,
            ForesterId = load.ForesterId,
            Amount = load.TotalAmount,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = PaymentStatus.Unpaid,
            InvoiceNumber = invoiceNumber
        };
        _context.Payments.Add(payment);
    }
}

        load.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var loadDto = new LoadResponseDto
        {
            LoadId = load.LoadId,
            LoadNumber = load.LoadNumber,
            OrderId = load.OrderId,
            OrderNumber = load.Order?.OrderNumber,
            CustomerId = load.CustomerId,
            CustomerName = load.Customer != null ? load.Customer.User.FirstName + " " + load.Customer.User.LastName : "",
            DriverId = load.DriverId,
            DriverName = load.Driver != null ? load.Driver.User.FirstName + " " + load.Driver.User.LastName : "",
            ForesterId = load.ForesterId,
            ForesterName = load.Forester != null ? load.Forester.User.FirstName + " " + load.Forester.User.LastName : "",
            WoodType = load.WoodType,
            Volume = load.Volume,
            PricePerCubicMeter = load.PricePerCubicMeter,
            TotalAmount = load.TotalAmount,
            DeliveryLocation = load.DeliveryLocation,
            Notes = load.Notes,
            Status = load.Status,
            PaymentStatus = load.PaymentStatus,
            BeforeLoadPhoto = load.BeforeLoadPhoto,
            OnTruckPhoto = load.OnTruckPhoto,
            DeliveredPhoto = load.DeliveredPhoto,
            CreatedAt = load.CreatedAt,
            LoadedAt = load.LoadedAt,
            DeliveredAt = load.DeliveredAt
        };

        return new ApiResponse<LoadResponseDto>
        {
            Success = true,
            Message = "Load status updated successfully",
            Data = loadDto
        };
    }

    public async Task<ApiResponse<LoadResponseDto>> UploadPhotoAsync(Guid loadId, Guid driverId, UploadPhotoDto dto)
    {
        var load = await _context.Loads
            .Include(l => l.Order)
            .Include(l => l.Customer)
                .ThenInclude(c => c!.User)
            .Include(l => l.Driver)
                .ThenInclude(d => d!.User)
            .Include(l => l.Forester)
                .ThenInclude(f => f!.User)
            .FirstOrDefaultAsync(l => l.LoadId == loadId);

        if (load == null)
        {
            return new ApiResponse<LoadResponseDto>
            {
                Success = false,
                Message = "Load not found"
            };
        }

        if (load.DriverId != driverId)
        {
            return new ApiResponse<LoadResponseDto>
            {
                Success = false,
                Message = "You can only upload photos for your own loads"
            };
        }

        switch (dto.PhotoType.ToLower())
        {
            case "before":
                load.BeforeLoadPhoto = dto.PhotoUrl;
                break;
            case "ontruck":
                load.OnTruckPhoto = dto.PhotoUrl;
                break;
            case "delivered":
                load.DeliveredPhoto = dto.PhotoUrl;
                break;
            default:
                return new ApiResponse<LoadResponseDto>
                {
                    Success = false,
                    Message = "Invalid photo type. Use: before, ontruck, or delivered"
                };
        }

        load.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var loadDto = new LoadResponseDto
        {
            LoadId = load.LoadId,
            LoadNumber = load.LoadNumber,
            OrderId = load.OrderId,
            OrderNumber = load.Order?.OrderNumber,
            CustomerId = load.CustomerId,
            CustomerName = load.Customer != null ? load.Customer.User.FirstName + " " + load.Customer.User.LastName : "",
            DriverId = load.DriverId,
            DriverName = load.Driver != null ? load.Driver.User.FirstName + " " + load.Driver.User.LastName : "",
            ForesterId = load.ForesterId,
            ForesterName = load.Forester != null ? load.Forester.User.FirstName + " " + load.Forester.User.LastName : "",
            WoodType = load.WoodType,
            Volume = load.Volume,
            PricePerCubicMeter = load.PricePerCubicMeter,
            TotalAmount = load.TotalAmount,
            DeliveryLocation = load.DeliveryLocation,
            Notes = load.Notes,
            Status = load.Status,
            PaymentStatus = load.PaymentStatus,
            BeforeLoadPhoto = load.BeforeLoadPhoto,
            OnTruckPhoto = load.OnTruckPhoto,
            DeliveredPhoto = load.DeliveredPhoto,
            CreatedAt = load.CreatedAt,
            LoadedAt = load.LoadedAt,
            DeliveredAt = load.DeliveredAt
        };

        return new ApiResponse<LoadResponseDto>
        {
            Success = true,
            Message = "Photo uploaded successfully",
            Data = loadDto
        };
    }
}