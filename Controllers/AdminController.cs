using Microsoft.AspNetCore.Mvc;

namespace HealthCare_Portal.Controllers;

public class AdminController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Dashboard() => View();
    public IActionResult Profile() => View("Profile");
    public IActionResult Patients(string page = "Index") => View($"Patients/{page}");
    public IActionResult Doctors(string page = "Index") => View($"Doctors/{page}");
    public IActionResult Receptionists(string page = "Index") => View($"Receptionists/{page}");
    public IActionResult Departments(string page = "Index") => View($"Departments/{page}");
    public IActionResult Specializations(string page = "Index") => View($"Specializations/{page}");
    public IActionResult Schedules(string page = "Index") => View($"Schedules/{page}");
    public IActionResult Appointments(string page = "Index") => View($"Appointments/{page}");
    public IActionResult MedicalRecords(string page = "Index") => View($"MedicalRecords/{page}");
    public IActionResult Prescriptions(string page = "Index") => View($"Prescriptions/{page}");
    public IActionResult MedicalTests(string page = "Index") => View($"MedicalTests/{page}");
    public IActionResult Billing(string page = "Index") => View($"Billing/{page}");
    public IActionResult Payments(string page = "Index") => View($"Payments/{page}");
    public IActionResult Invoices(string page = "Index") => View($"Invoices/{page}");
    public IActionResult Reports(string page = "Appointments") => View($"Reports/{page}");
    public IActionResult Notifications(string page = "Index") => View($"Notifications/{page}");
    public IActionResult AuditLogs(string page = "Index") => View($"AuditLogs/{page}");
    public IActionResult Settings(string page = "Index") => View($"Settings/{page}");
}
