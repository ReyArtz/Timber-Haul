using System.ComponentModel.DataAnnotations;
using TimberHaul.API.Models;

namespace TimberHaul.API.DTOs;

public class CreatePaymentDto
{
    [Required(ErrorMessage = "Load ID is required")]
    public Guid LoadId { get; set; }

    [Required(ErrorMessage = "Due date is required")]
    public DateTime DueDate { get; set; }

    public string? Notes { get; set; }
}

public class RecordPaymentDto
{
    [Required(ErrorMessage = "Payment method is required")]
    public PaymentMethod PaymentMethod { get; set; }

    public string? Notes { get; set; }
}

public class PaymentResponseDto
{
    public Guid PaymentId { get; set; }
    public Guid? LoadId { get; set; }
    public string? LoadNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? ForesterId { get; set; }
    public string? ForesterName { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod? PaymentMethodType { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime DueDate { get; set; }
    public PaymentStatus Status { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentListDto
{
    public Guid PaymentId { get; set; }
    public string? LoadNumber { get; set; }
    public string? CustomerName { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public PaymentStatus Status { get; set; }
    public string? InvoiceNumber { get; set; }
}

public class PaymentSummaryDto
{
    public int TotalPayments { get; set; }
    public int PaidCount { get; set; }
    public int UnpaidCount { get; set; }
    public int OverdueCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal UnpaidAmount { get; set; }
    public decimal OverdueAmount { get; set; }
}