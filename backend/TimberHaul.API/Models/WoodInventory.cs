using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("wood_inventory")]
public class WoodInventory
{
    [Key]
    [Column("inventory_id")]
    public Guid InventoryId { get; set; } = Guid.NewGuid();

    [Column("forester_id")]
    public Guid ForesterId { get; set; }

    [Column("plot_id")]
    public Guid? PlotId { get; set; }

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Required]
    [Column("available_volume")]
    public decimal AvailableVolume { get; set; }

    [Column("reserved_volume")]
    public decimal ReservedVolume { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ForesterId")]
    public ForesterProfile Forester { get; set; } = null!;

    [ForeignKey("PlotId")]
    public ForestPlot? Plot { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;
}