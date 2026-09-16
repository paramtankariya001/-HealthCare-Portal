using Microsoft.AspNetCore.Mvc;

namespace HealthCare_Portal.Controllers;

public class AccountController : Controller
{
    public IActionResult Login() => View();
    public IActionResult Register() => View();
    public IActionResult ForgotPassword() => View();
}