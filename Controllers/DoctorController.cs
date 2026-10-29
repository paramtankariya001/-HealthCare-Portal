using Microsoft.AspNetCore.Mvc;
using HealthCare_Portal.Filters;
using HealthCare_Portal.Helpers;
using HealthCare_Portal.Models;
using HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Controllers;

[AuthorizeRole("Doctor")]
public class DoctorController(
    SessionHelper session,
    IDoctorService doctors,
    IAppointmentService appointments,
    IPatientService patients,
    IMedicalService medical,
    INotificationService notifications) : Controller
{
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var docResult = await doctors.GetByIdAsync(doctorId.Value, cancellationToken);
        var apptsResult = await appointments.GetByDoctorAsync(doctorId.Value, cancellationToken);
        var statsResult = await doctors.GetDashboardStatsAsync(doctorId.Value, cancellationToken);

        ViewData["Doctor"] = docResult.Data;
        ViewData["Appointments"] = apptsResult.Data ?? Array.Empty<Appointment>();
        ViewData["Stats"] = statsResult.Data ?? new Dictionary<string, int>();

        return View();
    }

    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var docResult = await doctors.GetByIdAsync(doctorId.Value, cancellationToken);
        return docResult.Success ? View(docResult.Data) : NotFound();
    }

    public IActionResult Schedule() => View();

    public async Task<IActionResult> Appointments(CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var appts = (await appointments.GetByDoctorAsync(doctorId.Value, cancellationToken)).Data ?? Array.Empty<Appointment>();
        ViewData["Appointments"] = appts;
        return View(appts);
    }

    public async Task<IActionResult> AppointmentDetails(long id, CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var result = await appointments.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null || result.Data.DoctorId != doctorId.Value)
            return NotFound();

        var consultationResult = await medical.GetConsultationByAppointmentAsync(id, doctorId.Value, cancellationToken);
        ViewData["Consultation"] = consultationResult.Data;

        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Consultation(long? appointmentId, CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var model = new ConsultationViewModel();
        if (appointmentId.HasValue)
        {
            var apptResult = await appointments.GetByIdAsync(appointmentId.Value, cancellationToken);
            if (!apptResult.Success || apptResult.Data == null || apptResult.Data.DoctorId != doctorId.Value)
                return NotFound();

            var appt = apptResult.Data;
            model.AppointmentId = appt.AppointmentId;

            // Check if consultation already exists
            var existing = await medical.GetConsultationByAppointmentAsync(appt.AppointmentId, doctorId.Value, cancellationToken);
            if (existing.Success && existing.Data != null)
            {
                var c = existing.Data;
                model.Symptoms = c.Symptoms;
                model.BloodPressure = c.BloodPressure;
                model.Temperature = c.Temperature;
                model.Weight = c.Weight;
                model.Pulse = c.PulseRate;
                model.Diagnosis = c.Diagnosis;
                model.Notes = c.DoctorNotes;
                model.FollowUpDate = c.FollowUpDate;
                ViewData["IsExisting"] = true;
            }

            ViewData["Appointment"] = appt;

            // Load past consultations for this patient
            var history = await medical.GetConsultationsByPatientAsync(appt.PatientId, doctorId.Value, cancellationToken);
            ViewData["PastConsultations"] = history.Data ?? Array.Empty<Consultation>();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Consultation(ConsultationViewModel model, CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        if (!ModelState.IsValid)
        {
            var apptResult = await appointments.GetByIdAsync(model.AppointmentId, cancellationToken);
            ViewData["Appointment"] = apptResult.Data;
            return View(model);
        }

        var result = await medical.SaveConsultationAsync(doctorId.Value, model, cancellationToken);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            var apptResult = await appointments.GetByIdAsync(model.AppointmentId, cancellationToken);
            ViewData["Appointment"] = apptResult.Data;
            return View(model);
        }

        TempData["SuccessMessage"] = "Consultation notes saved successfully!";
        return RedirectToAction(nameof(AppointmentDetails), new { id = model.AppointmentId });
    }

    [HttpGet]
    public async Task<IActionResult> CreatePrescription(long? appointmentId, CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var model = new PrescriptionViewModel();
        if (appointmentId.HasValue)
        {
            var apptResult = await appointments.GetByIdAsync(appointmentId.Value, cancellationToken);
            if (!apptResult.Success || apptResult.Data == null || apptResult.Data.DoctorId != doctorId.Value)
                return NotFound();

            model.AppointmentId = appointmentId.Value;
            ViewData["Appointment"] = apptResult.Data;

            // Auto-populate diagnosis from consultation if recorded
            var consultation = await medical.GetConsultationByAppointmentAsync(appointmentId.Value, doctorId.Value, cancellationToken);
            if (consultation.Success && consultation.Data != null)
            {
                model.Diagnosis = consultation.Data.Diagnosis;
                model.FollowUpDate = consultation.Data.FollowUpDate;
            }
        }

        if (model.PrescriptionItems.Count == 0)
        {
            model.PrescriptionItems.Add(new PrescriptionItemViewModel());
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePrescription(PrescriptionViewModel model, CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        if (!ModelState.IsValid)
        {
            var apptResult = await appointments.GetByIdAsync(model.AppointmentId, cancellationToken);
            ViewData["Appointment"] = apptResult.Data;
            return View(model);
        }

        var result = await medical.CreatePrescriptionAsync(doctorId.Value, model, cancellationToken);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            var apptResult = await appointments.GetByIdAsync(model.AppointmentId, cancellationToken);
            ViewData["Appointment"] = apptResult.Data;
            return View(model);
        }

        TempData["SuccessMessage"] = "Prescription created and saved successfully!";
        return RedirectToAction(nameof(PrescriptionHistory));
    }

    public IActionResult Prescription() => RedirectToAction(nameof(CreatePrescription));

    public async Task<IActionResult> PrescriptionHistory(int? patientId, CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        IReadOnlyList<Prescription> list;
        if (patientId.HasValue)
        {
            var res = await medical.GetPrescriptionsByPatientAsync(patientId.Value, doctorId.Value, cancellationToken);
            list = res.Data ?? Array.Empty<Prescription>();
        }
        else
        {
            var res = await medical.GetPrescriptionsByDoctorAsync(doctorId.Value, cancellationToken);
            list = res.Data ?? Array.Empty<Prescription>();
        }

        ViewData["Prescriptions"] = list;
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> MedicalTests(long? appointmentId, CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        if (appointmentId.HasValue)
        {
            var apptResult = await appointments.GetByIdAsync(appointmentId.Value, cancellationToken);
            if (apptResult.Success && apptResult.Data != null && apptResult.Data.DoctorId == doctorId.Value)
            {
                ViewData["Appointment"] = apptResult.Data;
            }
        }

        var testsResult = await medical.GetMedicalTestsByDoctorAsync(doctorId.Value, cancellationToken);
        var tests = testsResult.Data ?? Array.Empty<MedicalTest>();
        ViewData["MedicalTests"] = tests;

        return View(tests);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestMedicalTest(long appointmentId, string testName, string priority, string? instructions, DateTime testDate, CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var result = await medical.RequestMedicalTestAsync(doctorId.Value, appointmentId, testName, priority, instructions, testDate, cancellationToken);
        if (!result.Success)
            TempData["ErrorMessage"] = result.Message;
        else
            TempData["SuccessMessage"] = "Medical test request submitted successfully!";

        return RedirectToAction(nameof(MedicalTests), new { appointmentId });
    }

    public async Task<IActionResult> TestDetails(long id, CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var result = await medical.GetMedicalTestByIdAsync(id, doctorId.Value, cancellationToken);
        if (!result.Success || result.Data == null)
            return NotFound();

        return View(result.Data);
    }

    public async Task<IActionResult> PatientDetails(int id, CancellationToken cancellationToken)
    {
        var doctorId = session.GetDoctorId();
        if (!doctorId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var patientResult = await patients.GetByIdAsync(id, cancellationToken);
        if (!patientResult.Success || patientResult.Data == null)
            return NotFound();

        var consultations = await medical.GetConsultationsByPatientAsync(id, doctorId.Value, cancellationToken);
        var prescriptions = await medical.GetPrescriptionsByPatientAsync(id, doctorId.Value, cancellationToken);
        var tests = await medical.GetMedicalTestsByPatientAsync(id, doctorId.Value, cancellationToken);

        ViewData["Patient"] = patientResult.Data;
        ViewData["Consultations"] = consultations.Data ?? Array.Empty<Consultation>();
        ViewData["Prescriptions"] = prescriptions.Data ?? Array.Empty<Prescription>();
        ViewData["MedicalTests"] = tests.Data ?? Array.Empty<MedicalTest>();

        return View(patientResult.Data);
    }

    public IActionResult MedicalHistory() => View();
    public async Task<IActionResult> Notifications(CancellationToken cancellationToken)
    {
        var userId = session.GetUserId();
        if (!userId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var result = await notifications.GetUserNotificationsAsync(userId.Value, cancellationToken: cancellationToken);
        return View(result.Data ?? Array.Empty<Notification>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationRead(long id, CancellationToken cancellationToken)
    {
        var userId = session.GetUserId();
        if (userId.HasValue)
            await notifications.MarkAsReadAsync(id, userId.Value, cancellationToken);

        return RedirectToAction(nameof(Notifications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllNotificationsRead(CancellationToken cancellationToken)
    {
        var userId = session.GetUserId();
        if (userId.HasValue)
            await notifications.MarkAllAsReadAsync(userId.Value, cancellationToken);

        return RedirectToAction(nameof(Notifications));
    }
}
