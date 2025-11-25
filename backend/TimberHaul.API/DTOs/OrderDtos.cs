using System.ComponentModel.DataAnnotations;
using TimberHaul.API.Models;

namespace TimberHaul.API.DTOs;

public class CreateOrderDto
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Volume is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Volume must be greater than 0")]
    public decimal Volume { get; set; }

    [Required(ErrorMessage = "Delivery address is required")]
    public string DeliveryAddress { get; set; } = string.Empty;

    public string? DeliveryCity { get; set; }

    public string? DeliveryPostalCode { get; set; }

    public string? CustomerNotes { get; set; }
}

public class OrderResponseDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public WoodType WoodType { get; set; }
    public Guid ForesterId { get; set; }
    public string ForesterName { get; set; } = string.Empty;
    public decimal Volume { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal TotalAmount { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? DeliveryCity { get; set; }
    public string? DeliveryPostalCode { get; set; }
    public string? CustomerNotes { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}

public class OrderListDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Volume { get; set; }
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateOrderStatusDto
{
    [Required(ErrorMessage = "Order status is required")]
    public string OrderStatus { get; set; } = string.Empty;
}