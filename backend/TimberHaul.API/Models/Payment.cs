using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimberHaul.API.Models;

[Table("payments")]
public class Payment
{
    [Key]
    [Column("payment_id")]
    public Guid PaymentId { get; set; } = Guid.NewGuid();

    [Column("load_id")]
    public Guid? LoadId { get; set; }

    [Column("customer_id")]
    public Guid? CustomerId { get; set; }

    [Column("forester_id")]
    public Guid? ForesterId { get; set; }

    [Required]
    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("payment_method")]
    public PaymentMethod? PaymentMethodType { get; set; }

    [Column("payment_date")]
    public DateTime? PaymentDate { get; set; }

    [Required]
    [Column("due_date")]
    public DateTime DueDate { get; set; }

    [Column("status")]
    public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;

    [MaxLength(50)]
    [Column("invoice_number")]
    public string? InvoiceNumber { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("LoadId")]
    public Load? Load { get; set; }

    [ForeignKey("CustomerId")]
    public CustomerProfile? Customer { get; set; }

    [ForeignKey("ForesterId")]
    public ForesterProfile? Forester { get; set; }
}