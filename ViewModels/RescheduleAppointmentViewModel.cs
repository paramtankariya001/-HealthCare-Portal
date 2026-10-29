using System.ComponentModel.DataAnnotations;
using HealthCare_Portal.Models;

namespace HealthCare_Portal.ViewModels;

public sealed class RescheduleAppointmentViewModel
{
    public long AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = "";
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public string? SpecializationName { get; set; }
    public decimal ConsultationFee { get; set; }
    public DateTime CurrentDate { get; set; }
    public TimeSpan CurrentStartTime { get; set; }
    public TimeSpan CurrentEndTime { get; set; }
    public string? ReasonForVisit { get; set; }

    [Required(ErrorMessage = "New appointment date is required."), DataType(DataType.Date)]
    public DateTime NewDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Please select a new time slot.")]
    public TimeSpan NewStartTime { get; set; }

    public TimeSpan? NewEndTime { get; set; }

    public IReadOnlyList<AppointmentSlot> AvailableSlots { get; set; } = Array.Empty<AppointmentSlot>();
}
