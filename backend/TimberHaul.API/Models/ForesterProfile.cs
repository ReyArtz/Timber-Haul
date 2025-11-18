using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("forester_profiles")]
public class ForesterProfile
{
    [Key]
    [Column("forester_id")]
    public Guid ForesterId { get; set; }

    [MaxLength(255)]
    [Column("company_name")]
    public string? CompanyName { get; set; }

    [MaxLength(50)]
    [Column("tax_id")]
    public string? TaxId { get; set; }

    [Column("total_wood_available")]
    public decimal TotalWoodAvailable { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ForesterId")]
    public User User { get; set; } = null!;

    public ICollection<ForestPlot> ForestPlots { get; set; } = new List<ForestPlot>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<WoodInventory> WoodInventories { get; set; } = new List<WoodInventory>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Load> Loads { get; set; } = new List<Load>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}