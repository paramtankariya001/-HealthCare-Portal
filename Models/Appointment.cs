namespace HealthCare_Portal.Models;

public sealed class Appointment
{
    public long AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = "";
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int DepartmentId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? ReasonForVisit { get; set; }
    public string AppointmentStatus { get; set; } = "";
    public string BookingSource { get; set; } = "";
    public decimal ConsultationFee { get; set; }
    public string? DoctorName { get; set; }
    public string? DoctorCode { get; set; }
    public string? DepartmentName { get; set; }
    public string? SpecializationName { get; set; }
    public string? PatientName { get; set; }
    public string? PatientCode { get; set; }
    public string? PatientPhone { get; set; }
    public DateTime? CreatedAt { get; set; }
}
