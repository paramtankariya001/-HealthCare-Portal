namespace HealthCare_Portal.Models;
public sealed class MedicalTestReport { public long ReportId { get; set; } public long MedicalTestId { get; set; } public string? ReportTitle { get; set; } public string? ResultSummary { get; set; } public string? ReportFilePath { get; set; } public string? ReportedBy { get; set; } public DateTime? ReportDate { get; set; } }
