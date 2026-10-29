using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HealthCare_Portal.ViewModels;

public sealed class DoctorFormViewModel
{
    public int DoctorId { get; set; }

    [Display(Name = "Doctor Code")]
    public string? DoctorCode { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = "";

    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Gender is required.")]
    public string Gender { get; set; } = "Male";

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(20)]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Qualification is required.")]
    [StringLength(250)]
    public string Qualification { get; set; } = "";

    [Required(ErrorMessage = "Department is required.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Specialization is required.")]
    [Display(Name = "Specialization")]
    public int SpecializationId { get; set; }

    [Required(ErrorMessage = "Experience is required.")]
    [Range(0, 70, ErrorMessage = "Experience must be between 0 and 70 years.")]
    [Display(Name = "Experience (Years)")]
    public int ExperienceYears { get; set; }

    [Required(ErrorMessage = "Consultation fee is required.")]
    [Range(0, 100000, ErrorMessage = "Consultation fee must be non-negative.")]
    [Display(Name = "Consultation Fee (₹)")]
    public decimal ConsultationFee { get; set; }

    [StringLength(2000)]
    [Display(Name = "About Doctor / Notes")]
    public string? AboutDoctor { get; set; }

    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;

    public List<SelectListItem> DepartmentList { get; set; } = new();
    public List<SelectListItem> SpecializationList { get; set; } = new();
}
