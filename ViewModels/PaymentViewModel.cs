using System.ComponentModel.DataAnnotations;

namespace HealthCare_Portal.ViewModels;

public sealed class PaymentViewModel
{
    [Range(1, long.MaxValue, ErrorMessage = "Please select a valid bill.")]
    public long BillId { get; set; }

    public string? InvoiceNumber { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }

    [Range(typeof(decimal), "0.01", "100000000", ErrorMessage = "Payment amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Please select a payment method.")]
    public string PaymentMethod { get; set; } = "Cash";

    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
