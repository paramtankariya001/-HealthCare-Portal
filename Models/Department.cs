namespace HealthCare_Portal.Models;
public sealed class Department { public int DepartmentId { get; set; } public string DepartmentName { get; set; } = ""; public string? Description { get; set; } public bool IsActive { get; set; } public DateTime CreatedAt { get; set; } }
