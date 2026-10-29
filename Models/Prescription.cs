namespace HealthCare_Portal.Models;

public sealed class Prescription
{
    public long PrescriptionId { get; set; }
    public string PrescriptionCode { get; set; } = "";
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public long? AppointmentId { get; set; }
    public long? ConsultationId { get; set; }
    public string? Diagnosis { get; set; }
    public string? Instructions { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public string? PatientName { get; set; }
    public string? PatientCode { get; set; }
    public string? DoctorName { get; set; }
    public List<PrescriptionItem> Items { get; set; } = new();
}
