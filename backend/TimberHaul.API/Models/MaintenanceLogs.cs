using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("maintenance_log")]
public class MaintenanceLog
{
    [Key]
    [Column("log_id")]
    public Guid LogId { get; set; } = Guid.NewGuid();

    [Column("equipment_id")]
    public Guid EquipmentId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("maintenance_type")]
    public string MaintenanceType { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("cost")]
    public decimal? Cost { get; set; }

    [Column("performed_at")]
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    [Column("performed_by")]
    public Guid? PerformedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("EquipmentId")]
    public Equipment Equipment { get; set; } = null!;

    [ForeignKey("PerformedBy")]
    public User? PerformedByUser { get; set; }
}