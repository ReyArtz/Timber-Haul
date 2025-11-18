using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("loads")]
public class Load
{
    [Key]
    [Column("load_id")]
    public Guid LoadId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    [Column("load_number")]
    public string LoadNumber { get; set; } = string.Empty;

    [Column("order_id")]
    public Guid? OrderId { get; set; }

    [Column("forester_id")]
    public Guid? ForesterId { get; set; }

    [Column("customer_id")]
    public Guid? CustomerId { get; set; }

    [Column("driver_id")]
    public Guid? DriverId { get; set; }

    [Column("plot_id")]
    public Guid? PlotId { get; set; }

    [Column("product_id")]
    public Guid? ProductId { get; set; }

    [Required]
    [Column("wood_type")]
    public WoodType WoodType { get; set; }

    [Required]
    [Column("volume")]
    public decimal Volume { get; set; }

    [Required]
    [Column("price_per_cubic_meter")]
    public decimal PricePerCubicMeter { get; set; }

    [Required]
    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Required]
    [Column("delivery_location")]
    public string DeliveryLocation { get; set; } = string.Empty;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("status")]
    public LoadStatus Status { get; set; } = LoadStatus.Pending;

    [Column("payment_status")]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    [MaxLength(500)]
    [Column("before_load_photo")]
    public string? BeforeLoadPhoto { get; set; }

    [MaxLength(500)]
    [Column("on_truck_photo")]
    public string? OnTruckPhoto { get; set; }

    [MaxLength(500)]
    [Column("delivered_photo")]
    public string? DeliveredPhoto { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("loaded_at")]
    public DateTime? LoadedAt { get; set; }

    [Column("delivered_at")]
    public DateTime? DeliveredAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("OrderId")]
    public Order? Order { get; set; }

    [ForeignKey("ForesterId")]
    public ForesterProfile? Forester { get; set; }

    [ForeignKey("CustomerId")]
    public CustomerProfile? Customer { get; set; }

    [ForeignKey("DriverId")]
    public DeliveryProfile? Driver { get; set; }

    [ForeignKey("PlotId")]
    public ForestPlot? Plot { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}