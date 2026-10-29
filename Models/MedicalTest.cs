namespace HealthCare_Portal.Models;

public sealed class MedicalTest
{
    public long MedicalTestId { get; set; }
    public string TestCode { get; set; } = "";
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public long? AppointmentId { get; set; }
    public string TestName { get; set; } = "";
    public string Priority { get; set; } = "Normal";
    public string? Instructions { get; set; }
    public DateTime TestDate { get; set; }
    public string TestStatus { get; set; } = "";
    public string? PatientName { get; set; }
    public string? PatientCode { get; set; }
    public string? DoctorName { get; set; }
    public MedicalTestReport? Report { get; set; }
}
