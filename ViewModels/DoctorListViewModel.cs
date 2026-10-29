using Microsoft.AspNetCore.Mvc.Rendering;
using HealthCare_Portal.Models;

namespace HealthCare_Portal.ViewModels;

public sealed class DoctorListViewModel
{
    public IReadOnlyList<Doctor> Doctors { get; set; } = Array.Empty<Doctor>();
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public string? Status { get; set; } = "All";
    public List<SelectListItem> DepartmentList { get; set; } = new();
}
