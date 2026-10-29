namespace HealthCare_Portal.Models;

public sealed class Bill
{
    public long BillId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public string? PatientCode { get; set; }
    public string? PatientName { get; set; }
    public string? PatientPhone { get; set; }
    public string? PatientEmail { get; set; }
    public string? PatientAddress { get; set; }
    public long? AppointmentId { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string? DepartmentName { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount => GrandTotal - PaidAmount;
    public string PaymentStatus { get; set; } = "Pending";
    public DateTime BillDate { get; set; }
    public int? CreatedByUserId { get; set; }
    public List<BillItem> Items { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
}
