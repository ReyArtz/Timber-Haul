using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("cart_items")]
public class CartItem
{
    [Key]
    [Column("cart_item_id")]
    public Guid CartItemId { get; set; } = Guid.NewGuid();

    [Column("customer_id")]
    public Guid CustomerId { get; set; }

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("volume")]
    public decimal Volume { get; set; }

    [Required]
    [Column("price_per_unit")]
    public decimal PricePerUnit { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CustomerId")]
    public CustomerProfile Customer { get; set; } = null!;

    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;
}