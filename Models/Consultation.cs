namespace HealthCare_Portal.Models;

public sealed class Consultation
{
    public long ConsultationId { get; set; }
    public long AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string? Symptoms { get; set; }
    public string? BloodPressure { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? Weight { get; set; }
    public int? PulseRate { get; set; }
    public string? Diagnosis { get; set; }
    public string? DoctorNotes { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public DateTime ConsultationDate { get; set; }
    public string? PatientName { get; set; }
    public string? PatientCode { get; set; }
    public string? DoctorName { get; set; }
    public string? DepartmentName { get; set; }
}
