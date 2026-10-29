namespace HealthCare_Portal.Models;
public sealed class Role { public int RoleId { get; set; } public string RoleName { get; set; } = ""; public string? Description { get; set; } public bool IsActive { get; set; } public DateTime CreatedAt { get; set; } }
