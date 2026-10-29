namespace HealthCare_Portal.Models;

public sealed class Doctor
{
    public int DoctorId { get; set; }
    public int? UserId { get; set; }
    public string DoctorCode { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? LastName { get; set; }
    public string Gender { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Qualification { get; set; }
    public int SpecializationId { get; set; }
    public int? DepartmentId { get; set; }
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? AboutDoctor { get; set; }
    public string? SpecializationName { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}
