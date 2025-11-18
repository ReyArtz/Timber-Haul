using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("reviews")]
public class Review
{
    [Key]
    [Column("review_id")]
    public Guid ReviewId { get; set; } = Guid.NewGuid();

    [Column("load_id")]
    public Guid LoadId { get; set; }

    [Column("customer_id")]
    public Guid CustomerId { get; set; }

    [Column("driver_id")]
    public Guid? DriverId { get; set; }

    [Required]
    [Range(1, 5)]
    [Column("rating")]
    public int Rating { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("LoadId")]
    public Load Load { get; set; } = null!;

    [ForeignKey("CustomerId")]
    public CustomerProfile Customer { get; set; } = null!;

    [ForeignKey("DriverId")]
    public DeliveryProfile? Driver { get; set; }
}