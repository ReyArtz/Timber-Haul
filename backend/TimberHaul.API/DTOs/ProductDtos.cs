using System.ComponentModel.DataAnnotations;
using TimberHaul.API.Models;

namespace TimberHaul.API.DTOs;

public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [MaxLength(255)]
    public string ProductName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wood type is required")]
    public WoodType WoodType { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal PricePerUnit { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Minimum order volume must be greater than 0")]
    public decimal MinOrderVolume { get; set; } = 1.0m;

    [Required(ErrorMessage = "Available stock is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Stock cannot be negative")]
    public decimal AvailableStock { get; set; }

    public string? ProductImage { get; set; }
}

public class UpdateProductDto
{
    [MaxLength(255)]
    public string? ProductName { get; set; }

    public WoodType? WoodType { get; set; }

    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal? PricePerUnit { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Minimum order volume must be greater than 0")]
    public decimal? MinOrderVolume { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Stock cannot be negative")]
    public decimal? AvailableStock { get; set; }

    public string? ProductImage { get; set; }

    public bool? IsAvailable { get; set; }
}

public class ProductResponseDto
{
    public Guid ProductId { get; set; }
    public Guid ForesterId { get; set; }
    public string ForesterName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public WoodType WoodType { get; set; }
    public string? Description { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal MinOrderVolume { get; set; }
    public decimal AvailableStock { get; set; }
    public string? ProductImage { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
}