using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("delivery_profiles")]
public class DeliveryProfile
{
    [Key]
    [Column("driver_id")]
    public Guid DriverId { get; set; }

    [MaxLength(50)]
    [Column("license_number")]
    public string? LicenseNumber { get; set; }

    [MaxLength(20)]
    [Column("vehicle_plate")]
    public string? VehiclePlate { get; set; }

    [Column("total_deliveries")]
    public int TotalDeliveries { get; set; } = 0;

    [Column("rating")]
    public decimal Rating { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("DriverId")]
    public User User { get; set; } = null!;

    public ICollection<Load> Loads { get; set; } = new List<Load>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}