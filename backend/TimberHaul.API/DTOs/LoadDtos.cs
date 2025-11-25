using System.ComponentModel.DataAnnotations;
using TimberHaul.API.Models;

namespace TimberHaul.API.DTOs;

public class CreateLoadDto
{
    [Required(ErrorMessage = "Order ID is required")]
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "Driver ID is required")]
    public Guid DriverId { get; set; }

    public Guid? PlotId { get; set; }

    public string? Notes { get; set; }
}

public class UpdateLoadStatusDto
{
    [Required(ErrorMessage = "Status is required")]
    public LoadStatus Status { get; set; }

    public string? Notes { get; set; }
}

public class UploadPhotoDto
{
    [Required(ErrorMessage = "Photo type is required")]
    public string PhotoType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Photo URL is required")]
    public string PhotoUrl { get; set; } = string.Empty;
}

public class LoadResponseDto
{
    public Guid LoadId { get; set; }
    public string LoadNumber { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public Guid? ForesterId { get; set; }
    public string? ForesterName { get; set; }
    public string? PlotName { get; set; }
    public WoodType WoodType { get; set; }
    public decimal Volume { get; set; }
    public decimal PricePerCubicMeter { get; set; }
    public decimal TotalAmount { get; set; }
    public string DeliveryLocation { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public LoadStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? BeforeLoadPhoto { get; set; }
    public string? OnTruckPhoto { get; set; }
    public string? DeliveredPhoto { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LoadedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public class LoadListDto
{
    public Guid LoadId { get; set; }
    public string LoadNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Volume { get; set; }
    public string DeliveryLocation { get; set; } = string.Empty;
    public LoadStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}