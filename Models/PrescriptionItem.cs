namespace HealthCare_Portal.Models;
public sealed class PrescriptionItem { public long PrescriptionItemId { get; set; } public long PrescriptionId { get; set; } public string MedicineName { get; set; } = ""; public string? Dosage { get; set; } public string? Frequency { get; set; } public string? Duration { get; set; } public string? Instructions { get; set; } }
