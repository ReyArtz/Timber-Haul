using Microsoft.EntityFrameworkCore;
using TimberHaul.API.Data;
using TimberHaul.API.DTOs;
using TimberHaul.API.Models;

namespace TimberHaul.API.Services;

public interface IOrderService
{
    Task<ApiResponse<OrderResponseDto>> CreateOrderAsync(Guid customerId, CreateOrderDto dto);
    Task<ApiResponse<List<OrderListDto>>> GetMyOrdersAsync(Guid customerId);
    Task<ApiResponse<OrderResponseDto>> GetOrderByIdAsync(Guid orderId, Guid userId);
    Task<ApiResponse<List<OrderListDto>>> GetForesterOrdersAsync(Guid foresterId);
    Task<ApiResponse<OrderResponseDto>> UpdateOrderStatusAsync(Guid orderId, Guid foresterId, UpdateOrderStatusDto dto);
}

public class OrderService : IOrderService
{
    private readonly TimberHaulDbContext _context;

    public OrderService(TimberHaulDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<OrderResponseDto>> CreateOrderAsync(Guid customerId, CreateOrderDto dto)
    {
        var customer = await _context.CustomerProfiles
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (customer == null)
        {
            return new ApiResponse<OrderResponseDto>
            {
                Success = false,
                Message = "Customer profile not found"
            };
        }

        var product = await _context.Products
            .Include(p => p.Forester)
                .ThenInclude(f => f.User)
            .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId);

        if (product == null)
        {
            return new ApiResponse<OrderResponseDto>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        if (!product.IsAvailable)
        {
            return new ApiResponse<OrderResponseDto>
            {
                Success = false,
                Message = "Product is not available"
            };
        }

        if (dto.Volume < product.MinOrderVolume)
        {
            return new ApiResponse<OrderResponseDto>
            {
                Success = false,
                Message = $"Minimum order volume is {product.MinOrderVolume} m³"
            };
        }

        if (dto.Volume > product.AvailableStock)
        {
            return new ApiResponse<OrderResponseDto>
            {
                Success = false,
                Message = $"Only {product.AvailableStock} m³ available in stock"
            };
        }

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        var totalAmount = dto.Volume * product.PricePerUnit;

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = customerId,
            ProductId = dto.ProductId,
            ForesterId = product.ForesterId,
            Volume = dto.Volume,
            PricePerUnit = product.PricePerUnit,
            TotalAmount = totalAmount,
            DeliveryAddress = dto.DeliveryAddress,
            DeliveryCity = dto.DeliveryCity,
            DeliveryPostalCode = dto.DeliveryPostalCode,
            CustomerNotes = dto.CustomerNotes,
            OrderStatus = "pending"
        };

        _context.Orders.Add(order);

        product.AvailableStock -= dto.Volume;

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == dto.ProductId);
        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
        }

        await _context.SaveChangesAsync();

        var orderDto = new OrderResponseDto
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            CustomerId = customerId,
            CustomerName = customer.User.FirstName + " " + customer.User.LastName,
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            WoodType = product.WoodType,
            ForesterId = product.ForesterId,
            ForesterName = product.Forester.User.FirstName + " " + product.Forester.User.LastName,
            Volume = order.Volume,
            PricePerUnit = order.PricePerUnit,
            TotalAmount = order.TotalAmount,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryCity = order.DeliveryCity,
            DeliveryPostalCode = order.DeliveryPostalCode,
            CustomerNotes = order.CustomerNotes,
            OrderStatus = order.OrderStatus,
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt
        };

        return new ApiResponse<OrderResponseDto>
        {
            Success = true,
            Message = "Order created successfully",
            Data = orderDto
        };
    }

    public async Task<ApiResponse<List<OrderListDto>>> GetMyOrdersAsync(Guid customerId)
    {
        var orders = await _context.Orders
            .Include(o => o.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListDto
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                ProductName = o.Product!.ProductName,
                Volume = o.Volume,
                TotalAmount = o.TotalAmount,
                OrderStatus = o.OrderStatus,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return new ApiResponse<List<OrderListDto>>
        {
            Success = true,
            Message = "Orders retrieved successfully",
            Data = orders
        };
    }

    public async Task<ApiResponse<OrderResponseDto>> GetOrderByIdAsync(Guid orderId, Guid userId)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
            .Include(o => o.Product)
            .Include(o => o.Forester)
                .ThenInclude(f => f!.User)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            return new ApiResponse<OrderResponseDto>
            {
                Success = false,
                Message = "Order not found"
            };
        }

        if (order.CustomerId != userId && order.ForesterId != userId)
        {
            return new ApiResponse<OrderResponseDto>
            {
                Success = false,
                Message = "You don't have access to this order"
            };
        }

        var orderDto = new OrderResponseDto
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId ?? Guid.Empty,
            CustomerName = order.Customer != null ? order.Customer.User.FirstName + " " + order.Customer.User.LastName : "",
            ProductId = order.ProductId ?? Guid.Empty,
            ProductName = order.Product?.ProductName ?? "",
            WoodType = order.Product?.WoodType ?? WoodType.Other,
            ForesterId = order.ForesterId ?? Guid.Empty,
            ForesterName = order.Forester != null ? order.Forester.User.FirstName + " " + order.Forester.User.LastName : "",
            Volume = order.Volume,
            PricePerUnit = order.PricePerUnit,
            TotalAmount = order.TotalAmount,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryCity = order.DeliveryCity,
            DeliveryPostalCode = order.DeliveryPostalCode,
            CustomerNotes = order.CustomerNotes,
            OrderStatus = order.OrderStatus,
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt
        };

        return new ApiResponse<OrderResponseDto>
        {
            Success = true,
            Message = "Order retrieved successfully",
            Data = orderDto
        };
    }

    public async Task<ApiResponse<List<OrderListDto>>> GetForesterOrdersAsync(Guid foresterId)
    {
        var orders = await _context.Orders
            .Include(o => o.Product)
            .Where(o => o.ForesterId == foresterId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListDto
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                ProductName = o.Product!.ProductName,
                Volume = o.Volume,
                TotalAmount = o.TotalAmount,
                OrderStatus = o.OrderStatus,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return new ApiResponse<List<OrderListDto>>
        {
            Success = true,
            Message = "Orders retrieved successfully",
            Data = orders
        };
    }

    public async Task<ApiResponse<OrderResponseDto>> UpdateOrderStatusAsync(Guid orderId, Guid foresterId, UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
            .Include(o => o.Product)
            .Include(o => o.Forester)
                .ThenInclude(f => f!.User)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            return new ApiResponse<OrderResponseDto>
            {
                Success = false,
                Message = "Order not found"
            };
        }

        if (order.ForesterId != foresterId)
        {
            return new ApiResponse<OrderResponseDto>
            {
                Success = false,
                Message = "You can only update your own orders"
            };
        }

        order.OrderStatus = dto.OrderStatus;
        
        if (dto.OrderStatus == "confirmed" && order.ConfirmedAt == null)
        {
            order.ConfirmedAt = DateTime.UtcNow;
        }

        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var orderDto = new OrderResponseDto
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId ?? Guid.Empty,
            CustomerName = order.Customer != null ? order.Customer.User.FirstName + " " + order.Customer.User.LastName : "",
            ProductId = order.ProductId ?? Guid.Empty,
            ProductName = order.Product?.ProductName ?? "",
            WoodType = order.Product?.WoodType ?? WoodType.Other,
            ForesterId = order.ForesterId ?? Guid.Empty,
            ForesterName = order.Forester != null ? order.Forester.User.FirstName + " " + order.Forester.User.LastName : "",
            Volume = order.Volume,
            PricePerUnit = order.PricePerUnit,
            TotalAmount = order.TotalAmount,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryCity = order.DeliveryCity,
            DeliveryPostalCode = order.DeliveryPostalCode,
            CustomerNotes = order.CustomerNotes,
            OrderStatus = order.OrderStatus,
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt
        };

        return new ApiResponse<OrderResponseDto>
        {
            Success = true,
            Message = "Order status updated successfully",
            Data = orderDto
        };
    }
}