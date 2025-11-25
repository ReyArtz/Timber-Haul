using System.ComponentModel.DataAnnotations;
using TimberHaul.API.Models;

namespace TimberHaul.API.DTOs;

public class AddToCartDto
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Volume is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Volume must be greater than 0")]
    public decimal Volume { get; set; }
}

public class UpdateCartItemDto
{
    [Required(ErrorMessage = "Volume is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Volume must be greater than 0")]
    public decimal Volume { get; set; }
}

public class CartItemResponseDto
{
    public Guid CartItemId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public WoodType WoodType { get; set; }
    public decimal Volume { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ProductImage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CartSummaryDto
{
    public List<CartItemResponseDto> Items { get; set; } = new List<CartItemResponseDto>();
    public int TotalItems { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal TotalAmount { get; set; }
}