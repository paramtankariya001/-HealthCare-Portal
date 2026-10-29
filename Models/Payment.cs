namespace HealthCare_Portal.Models;

public sealed class Payment
{
    public long PaymentId { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public long BillId { get; set; }
    public string? InvoiceNumber { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientCode { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public DateTime PaymentDate { get; set; }
    public string PaymentStatus { get; set; } = "Successful";
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
