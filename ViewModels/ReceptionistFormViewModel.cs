using System.ComponentModel.DataAnnotations;

namespace HealthCare_Portal.ViewModels;

public sealed class ReceptionistFormViewModel
{
    public int ReceptionistId { get; set; }

    [Display(Name = "Employee Code")]
    [StringLength(30)]
    public string? EmployeeCode { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100)]
    public string FirstName { get; set; } = "";

    [StringLength(100)]
    public string? LastName { get; set; }

    [Required]
    public string Gender { get; set; } = "Female";

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [Display(Name = "Login ID")]
    [StringLength(100)]
    public string? Username { get; set; }

    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string? Password { get; set; }

    [DataType(DataType.Date)]
    public DateTime? JoiningDate { get; set; }

    public bool IsActive { get; set; } = true;
}
