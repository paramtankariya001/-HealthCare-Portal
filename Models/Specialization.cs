namespace HealthCare_Portal.Models;
public sealed class Specialization { public int SpecializationId { get; set; } public int DepartmentId { get; set; } public string SpecializationName { get; set; } = ""; public string? Description { get; set; } public bool IsActive { get; set; } }
