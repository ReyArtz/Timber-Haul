using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("orders")]
public class Order
{
    [Key]
    [Column("order_id")]
    public Guid OrderId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    [Column("order_number")]
    public string OrderNumber { get; set; } = string.Empty;

    [Column("customer_id")]
    public Guid? CustomerId { get; set; }

    [Column("product_id")]
    public Guid? ProductId { get; set; }

    [Column("forester_id")]
    public Guid? ForesterId { get; set; }

    [Required]
    [Column("volume")]
    public decimal Volume { get; set; }

    [Required]
    [Column("price_per_unit")]
    public decimal PricePerUnit { get; set; }

    [Required]
    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Required]
    [Column("delivery_address")]
    public string DeliveryAddress { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("delivery_city")]
    public string? DeliveryCity { get; set; }

    [MaxLength(20)]
    [Column("delivery_postal_code")]
    public string? DeliveryPostalCode { get; set; }

    [Column("customer_notes")]
    public string? CustomerNotes { get; set; }

    [MaxLength(50)]
    [Column("order_status")]
    public string OrderStatus { get; set; } = "pending";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("confirmed_at")]
    public DateTime? ConfirmedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CustomerId")]
    public CustomerProfile? Customer { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [ForeignKey("ForesterId")]
    public ForesterProfile? Forester { get; set; }

    public ICollection<Load> Loads { get; set; } = new List<Load>();
}