namespace HealthCare_Portal.Models;
public sealed class BillItem { public long BillItemId { get; set; } public long BillId { get; set; } public string ServiceName { get; set; } = ""; public string? Description { get; set; } public int Quantity { get; set; } public decimal UnitPrice { get; set; } public decimal TotalPrice { get; set; } }
