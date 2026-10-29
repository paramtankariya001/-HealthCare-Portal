using Microsoft.AspNetCore.Mvc;
using HealthCare_Portal.Helpers;
using HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Controllers;

public sealed class AccountController(IUserService userService, SessionHelper session) : Controller
{
    public IActionResult Login() => View();
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await userService.LoginAsync(model.Username, model.Password, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        var user = result.Data;
        session.SetUserSession(user.UserId, user.RoleId, user.RoleName ?? "", user.Username, user.PatientId, user.DoctorId, user.ReceptionistId);
        var destination = user.RoleName?.ToLowerInvariant() switch
        {
            "admin" => "Admin",
            "doctor" => "Doctor",
            "receptionist" => "Receptionist",
            "patient" => "Patient",
            _ => ""
        };
        if (string.IsNullOrEmpty(destination))
        {
            session.ClearSession();
            ModelState.AddModelError(string.Empty, "Your account role is not configured.");
            return View(model);
        }
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        return RedirectToAction("Dashboard", destination);
    }

    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterPatientViewModel model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.Username) && !string.IsNullOrWhiteSpace(model.Email))
        {
            model.Username = model.Email.Split('@')[0];
            ModelState.Remove(nameof(model.Username));
        }
        if (!ModelState.IsValid) return View(model);
        var result = await userService.RegisterPatientAsync(model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }
        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [ActionName("Logout")]
    public IActionResult LogoutGet()
    {
        session.ClearSession();
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        session.ClearSession();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult ForgotPassword() => View();
    public IActionResult AccessDenied() => View();
}
