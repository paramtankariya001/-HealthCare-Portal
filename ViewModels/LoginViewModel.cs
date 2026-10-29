using System.ComponentModel.DataAnnotations;
namespace HealthCare_Portal.ViewModels;
public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Login ID or email is required.")]
    [StringLength(150)]
    [Display(Name = "Login ID or Email")]
    public string Username { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}
