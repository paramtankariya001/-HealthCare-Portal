using Microsoft.AspNetCore.Mvc;

namespace HealthCare_Portal.Controllers;

public class DoctorController : Controller
{
    public IActionResult Dashboard() => View();
    public IActionResult Profile() => View();
    public IActionResult Schedule() => View();
    public IActionResult Appointments() => View();
    public IActionResult AppointmentDetails() => View();
    public IActionResult PatientDetails() => View();
    public IActionResult MedicalHistory() => View();
    public IActionResult Consultation() => View();
    public IActionResult Prescription() => RedirectToAction(nameof(CreatePrescription));
    public IActionResult CreatePrescription() => View();
    public IActionResult PrescriptionHistory() => View();
    public IActionResult MedicalTests() => View();
    public IActionResult TestDetails() => View();
    public IActionResult Notifications() => View();
}