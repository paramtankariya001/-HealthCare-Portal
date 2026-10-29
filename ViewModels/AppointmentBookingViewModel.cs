using System.ComponentModel.DataAnnotations;
using HealthCare_Portal.Models;

namespace HealthCare_Portal.ViewModels;

public sealed class AppointmentBookingViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid doctor.")]
    public int DoctorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department.")]
    public int DepartmentId { get; set; }

    public string? DoctorName { get; set; }
    public string? DepartmentName { get; set; }
    public string? SpecializationName { get; set; }
    public decimal ConsultationFee { get; set; }

    [Required(ErrorMessage = "Appointment date is required."), DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Please select an appointment time slot.")]
    public TimeSpan StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    [Required(ErrorMessage = "Reason for visit is required."), StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters.")]
    public string? Reason { get; set; }

    public IReadOnlyList<AppointmentSlot> AvailableSlots { get; set; } = Array.Empty<AppointmentSlot>();
    public IReadOnlyList<Doctor> DoctorList { get; set; } = Array.Empty<Doctor>();
}
