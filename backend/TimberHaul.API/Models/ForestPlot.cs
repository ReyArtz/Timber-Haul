using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("forest_plots")]
public class ForestPlot
{
    [Key]
    [Column("plot_id")]
    public Guid PlotId { get; set; } = Guid.NewGuid();

    [Column("forester_id")]
    public Guid ForesterId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("plot_name")]
    public string PlotName { get; set; } = string.Empty;

    [Required]
    [Column("location")]
    public string Location { get; set; } = string.Empty;

    [Column("total_area")]
    public decimal? TotalArea { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ForesterId")]
    public ForesterProfile Forester { get; set; } = null!;

    public ICollection<WoodInventory> WoodInventories { get; set; } = new List<WoodInventory>();
    public ICollection<Load> Loads { get; set; } = new List<Load>();
}