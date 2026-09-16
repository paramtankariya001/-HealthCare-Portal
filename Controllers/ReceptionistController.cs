using Microsoft.AspNetCore.Mvc;

namespace HealthCare_Portal.Controllers;

public class ReceptionistController : Controller
{
    public IActionResult Dashboard() => View();
    public IActionResult Profile() => View();
    public IActionResult PatientRegistration() => View();
    public IActionResult Patients() => View();
    public IActionResult PatientDetails() => View();
    public IActionResult Appointments() => View();
    public IActionResult AppointmentBooking() => View();
    public IActionResult AppointmentManagement() => View();
    public IActionResult BookAppointment() => View();
    public IActionResult AppointmentDetails() => View();
    public IActionResult CheckIn() => View();
    public IActionResult DoctorAvailability() => View();
    public IActionResult Billing() => View();
    public IActionResult Payments() => View();
    public IActionResult Payment() => View();
    public IActionResult Invoices() => View();
    public IActionResult InvoiceDetails() => View();
    public IActionResult Notifications() => View();
}