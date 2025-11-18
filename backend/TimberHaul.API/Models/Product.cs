using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("products")]
public class Product
{
    [Key]
    [Column("product_id")]
    public Guid ProductId { get; set; } = Guid.NewGuid();

    [Column("forester_id")]
    public Guid ForesterId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("product_name")]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Column("wood_type")]
    public WoodType WoodType { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("price_per_unit")]
    public decimal PricePerUnit { get; set; }

    [Column("min_order_volume")]
    public decimal MinOrderVolume { get; set; } = 1.0m;

    [Required]
    [Column("available_stock")]
    public decimal AvailableStock { get; set; }

    [MaxLength(500)]
    [Column("product_image")]
    public string? ProductImage { get; set; }

    [Column("is_available")]
    public bool IsAvailable { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ForesterId")]
    public ForesterProfile Forester { get; set; } = null!;

    public ICollection<WoodInventory> WoodInventories { get; set; } = new List<WoodInventory>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Load> Loads { get; set; } = new List<Load>();
}