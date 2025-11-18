using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("equipment")]
public class Equipment
{
    [Key]
    [Column("equipment_id")]
    public Guid EquipmentId { get; set; } = Guid.NewGuid();

    [Column("owner_id")]
    public Guid OwnerId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("equipment_name")]
    public string EquipmentName { get; set; } = string.Empty;

    [Required]
    [Column("equipment_type")]
    public EquipmentType EquipmentType { get; set; }

    [MaxLength(100)]
    [Column("model")]
    public string? Model { get; set; }

    [Column("runtime_hours")]
    public int RuntimeHours { get; set; } = 0;

    [Column("last_service_date")]
    public DateTime? LastServiceDate { get; set; }

    [Column("next_service_due")]
    public DateTime? NextServiceDue { get; set; }

    [Column("service_interval_hours")]
    public int? ServiceIntervalHours { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("OwnerId")]
    public User Owner { get; set; } = null!;

    public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();
}