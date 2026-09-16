using Microsoft.AspNetCore.Mvc;

namespace HealthCare_Portal.Controllers;

public class PatientController : Controller
{
    public IActionResult Dashboard() => View();
    public IActionResult Profile() => View();
    public IActionResult FindDoctor() => View();
    public IActionResult DoctorDetails() => View();
    public IActionResult BookAppointment() => View();
    public IActionResult Appointments() => View();
    public IActionResult AppointmentDetails() => View();
    public IActionResult MedicalHistory() => View();
    public IActionResult Prescriptions() => View();
    public IActionResult PrescriptionDetails() => View();
    public IActionResult MedicalTests() => View();
    public IActionResult TestReports() => View();
    public IActionResult Bills() => View();
    public IActionResult InvoiceDetails() => View();
    public IActionResult Notifications() => View();
}