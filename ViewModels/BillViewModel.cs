using System.ComponentModel.DataAnnotations;

namespace HealthCare_Portal.ViewModels;

public sealed class BillViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid patient.")]
    public int PatientId { get; set; }

    public string? PatientName { get; set; }
    public string? PatientCode { get; set; }

    public long? AppointmentId { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }

    public List<BillItemViewModel> BillItems { get; set; } = [];

    [Range(0, 1000000, ErrorMessage = "Discount must be non-negative.")]
    public decimal Discount { get; set; } = 0;

    [Range(0, 1000000, ErrorMessage = "Tax must be non-negative.")]
    public decimal Tax { get; set; } = 0;
}

public sealed class BillItemViewModel
{
    [Required(ErrorMessage = "Service name is required.")]
    public string ServiceName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(1, 1000, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; } = 1;

    [Range(0, 10000000, ErrorMessage = "Unit price must be non-negative.")]
    public decimal UnitPrice { get; set; } = 0;

    public decimal TotalPrice => Quantity * UnitPrice;
}
