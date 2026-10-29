using Microsoft.AspNetCore.Mvc;
using HealthCare_Portal.Filters;
using HealthCare_Portal.Helpers;
using HealthCare_Portal.Models;
using HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Controllers;

[AuthorizeRole("Patient")]
public class PatientController(
    SessionHelper session,
    IPatientService patients,
    IDoctorService doctors,
    IAppointmentService appointments,
    IBillingService billing,
    INotificationService notifications,
    IMedicalService medical,
    ILogger<PatientController> logger) : Controller
{
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var userId = session.GetUserId();
        var patientId = session.GetPatientId();
        if (!userId.HasValue || !patientId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var patient = await patients.GetByIdAsync(patientId.Value, cancellationToken);
        if (!patient.Success || patient.Data is null || patient.Data.UserId != userId.Value)
            return RedirectToAction("AccessDenied", "Account");

        try
        {
            var appointmentTask = appointments.GetByPatientAsync(patientId.Value, cancellationToken);
            var recordsTask = medical.GetMedicalRecordsByPatientAsync(patientId.Value, cancellationToken);
            var prescriptionsTask = medical.GetPatientPrescriptionsAsync(patientId.Value, cancellationToken);
            var billsTask = billing.GetByPatientAsync(patientId.Value, cancellationToken);
            var notificationsTask = notifications.GetUserNotificationsAsync(userId.Value, cancellationToken: cancellationToken);
            await Task.WhenAll(appointmentTask, recordsTask, prescriptionsTask, billsTask, notificationsTask);

            var model = new PatientDashboardViewModel
            {
                Patient = patient.Data,
                Appointments = appointmentTask.Result.Data ?? Array.Empty<Appointment>(),
                MedicalRecords = recordsTask.Result.Data ?? Array.Empty<MedicalRecord>(),
                Prescriptions = prescriptionsTask.Result.Data ?? Array.Empty<Prescription>(),
                Bills = billsTask.Result.Data ?? Array.Empty<Bill>(),
                Notifications = notificationsTask.Result.Data ?? Array.Empty<Notification>()
            };

            ViewData["Patient"] = model.Patient;
            ViewData["Notifications"] = model.Notifications;
            ViewData["UnreadNotifications"] = model.UnreadNotificationCount;
            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load dashboard data for patient {PatientId}.", patientId.Value);
            ViewData["Patient"] = patient.Data;
            ViewData["Notifications"] = Array.Empty<Notification>();
            ViewData["UnreadNotifications"] = 0;
            return View(new PatientDashboardViewModel { Patient = patient.Data });
        }
    }

    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var patientId = session.GetPatientId();
        if (!patientId.HasValue) return RedirectToAction("AccessDenied", "Account");
        var result = await patients.GetByIdAsync(patientId.Value, cancellationToken);
        return result.Success ? View(result.Data) : RedirectToAction("AccessDenied", "Account");
    }

    public async Task<IActionResult> FindDoctor(string? search, int? departmentId, int? specializationId, CancellationToken cancellationToken)
    {
        var searchResult = await doctors.SearchAsync(search, departmentId, specializationId, cancellationToken);
        ViewData["Doctors"] = searchResult.Data ?? Array.Empty<Doctor>();
        ViewData["Departments"] = await doctors.GetDepartmentsAsync(cancellationToken);
        ViewData["Search"] = search;
        ViewData["DepartmentId"] = departmentId;
        return View();
    }

    public async Task<IActionResult> DoctorDetails(int id, CancellationToken cancellationToken)
    {
        var result = await doctors.GetByIdAsync(id, cancellationToken);
        return result.Success ? View(result.Data) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> BookAppointment(int? doctorId, CancellationToken cancellationToken)
    {
        var patientId = session.GetPatientId();
        if (!patientId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var allDocs = (await doctors.GetAllAsync(cancellationToken)).Data ?? Array.Empty<Doctor>();
        var activeDocs = allDocs.Where(d => d.IsActive).ToList();

        var model = new AppointmentBookingViewModel
        {
            DoctorList = activeDocs,
            AppointmentDate = DateTime.Today
        };

        if (doctorId.HasValue)
        {
            var docResult = await doctors.GetByIdAsync(doctorId.Value, cancellationToken);
            if (docResult.Success && docResult.Data != null && docResult.Data.IsActive)
            {
                var doc = docResult.Data;
                model.DoctorId = doc.DoctorId;
                model.DoctorName = $"Dr. {doc.FirstName} {doc.LastName}".Trim();
                model.DepartmentId = doc.DepartmentId ?? 0;
                model.DepartmentName = doc.DepartmentName;
                model.SpecializationName = doc.SpecializationName;
                model.ConsultationFee = doc.ConsultationFee;

                var slots = await appointments.GetAvailableSlotsAsync(doc.DoctorId, model.AppointmentDate, cancellationToken);
                model.AvailableSlots = slots.Data ?? Array.Empty<AppointmentSlot>();
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date, CancellationToken cancellationToken)
    {
        var doctor = await doctors.GetByIdAsync(doctorId, cancellationToken);
        if (!doctor.Success || doctor.Data is null || !doctor.Data.IsActive)
            return Json(Array.Empty<AppointmentSlot>());

        var result = await appointments.GetAvailableSlotsAsync(doctorId, date, cancellationToken);
        var slots = result.Data ?? Array.Empty<AppointmentSlot>();
        return Json(slots.Select(s => new
        {
            slotStart = s.SlotStart.ToString(@"hh\:mm\:ss"),
            slotEnd = s.SlotEnd.ToString(@"hh\:mm\:ss"),
            formatted = s.FormattedSlot
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookAppointment(AppointmentBookingViewModel model, CancellationToken cancellationToken)
    {
        var patientId = session.GetPatientId();
        if (!patientId.HasValue) return RedirectToAction("AccessDenied", "Account");

        if (model.DoctorId > 0)
        {
            var docResult = await doctors.GetByIdAsync(model.DoctorId, cancellationToken);
            if (!docResult.Success || docResult.Data is null || !docResult.Data.IsActive)
            {
                ModelState.AddModelError(nameof(model.DoctorId), "Please select an active doctor.");
            }
            else
            {
                // The department is derived from the trusted doctor record, never from the browser.
                model.DepartmentId = docResult.Data.DepartmentId ?? 0;
            }
        }

        if (!ModelState.IsValid)
        {
            await ReloadBookingModelAsync(model, cancellationToken);
            return View(model);
        }

        var result = await appointments.BookAsync(patientId.Value, model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await ReloadBookingModelAsync(model, cancellationToken);
            return View(model);
        }

        TempData["SuccessMessage"] = "Appointment booked successfully!";
        return RedirectToAction(nameof(Appointments));
    }

    public async Task<IActionResult> Appointments(CancellationToken cancellationToken)
    {
        var patientId = session.GetPatientId();
        if (!patientId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var appts = (await appointments.GetByPatientAsync(patientId.Value, cancellationToken)).Data ?? Array.Empty<Appointment>();
        ViewData["Appointments"] = appts;
        return View(appts);
    }

    public async Task<IActionResult> AppointmentDetails(long id, CancellationToken cancellationToken)
    {
        var patientId = session.GetPatientId();
        if (!patientId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var result = await appointments.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null || result.Data.PatientId != patientId.Value)
            return NotFound();

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAppointment(long id, CancellationToken cancellationToken)
    {
        var patientId = session.GetPatientId();
        if (!patientId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var result = await appointments.CancelAsync(id, patientId.Value, cancellationToken);
        if (!result.Success)
            TempData["ErrorMessage"] = result.Message;
        else
            TempData["SuccessMessage"] = "Appointment cancelled successfully.";

        return RedirectToAction(nameof(Appointments));
    }

    [HttpGet]
    public async Task<IActionResult> RescheduleAppointment(long id, CancellationToken cancellationToken)
    {
        var patientId = session.GetPatientId();
        if (!patientId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var result = await appointments.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null || result.Data.PatientId != patientId.Value)
            return NotFound();

        var appt = result.Data;
        if (appt.AppointmentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
            appt.AppointmentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
            appt.AppointmentStatus.Equals("No Show", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Cannot reschedule a cancelled or completed appointment.";
            return RedirectToAction(nameof(Appointments));
        }

        var model = new RescheduleAppointmentViewModel
        {
            AppointmentId = appt.AppointmentId,
            AppointmentCode = appt.AppointmentCode,
            DoctorId = appt.DoctorId,
            DoctorName = appt.DoctorName ?? "Doctor",
            DepartmentName = appt.DepartmentName ?? "",
            SpecializationName = appt.SpecializationName,
            ConsultationFee = appt.ConsultationFee,
            CurrentDate = appt.AppointmentDate,
            CurrentStartTime = appt.StartTime,
            CurrentEndTime = appt.EndTime,
            ReasonForVisit = appt.ReasonForVisit,
            NewDate = appt.AppointmentDate >= DateTime.Today ? appt.AppointmentDate : DateTime.Today
        };

        var slotsResult = await appointments.GetAvailableSlotsAsync(appt.DoctorId, model.NewDate, cancellationToken);
        model.AvailableSlots = slotsResult.Data ?? Array.Empty<AppointmentSlot>();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RescheduleAppointment(RescheduleAppointmentViewModel model, CancellationToken cancellationToken)
    {
        var patientId = session.GetPatientId();
        if (!patientId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var end = model.NewEndTime ?? model.NewStartTime.Add(TimeSpan.FromMinutes(30));
        var result = await appointments.RescheduleAsync(model.AppointmentId, patientId.Value, model.NewDate, model.NewStartTime, end, cancellationToken);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            var slotsResult = await appointments.GetAvailableSlotsAsync(model.DoctorId, model.NewDate, cancellationToken);
            model.AvailableSlots = slotsResult.Data ?? Array.Empty<AppointmentSlot>();
            return View(model);
        }

        TempData["SuccessMessage"] = "Appointment rescheduled successfully.";
        return RedirectToAction(nameof(Appointments));
    }

    public IActionResult MedicalHistory() => View();
    public IActionResult Prescriptions() => View();
    public IActionResult PrescriptionDetails() => View();
    public IActionResult MedicalTests() => View();
    public IActionResult TestReports() => View();

    public async Task<IActionResult> Bills(CancellationToken cancellationToken)
    {
        var patientId = session.GetPatientId();
        if (!patientId.HasValue) return RedirectToAction("AccessDenied", "Account");
        var result = await billing.GetByPatientAsync(patientId.Value, cancellationToken);
        var bills = result.Data ?? Array.Empty<Bill>();
        ViewData["Bills"] = bills;
        return View(bills);
    }

    public async Task<IActionResult> InvoiceDetails(long id, CancellationToken cancellationToken)
    {
        var patientId = session.GetPatientId();
        if (!patientId.HasValue) return RedirectToAction("AccessDenied", "Account");

        var result = await billing.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null)
            return NotFound();

        // Security ownership validation: patient cannot view another patient's invoice
        if (result.Data.PatientId != patientId.Value)
            return RedirectToAction("AccessDenied", "Account");

        return View(result.Data);
    }

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

    private async Task ReloadBookingModelAsync(AppointmentBookingViewModel model, CancellationToken ct)
    {
        var allDocs = (await doctors.GetAllAsync(ct)).Data ?? Array.Empty<Doctor>();
        model.DoctorList = allDocs.Where(d => d.IsActive).ToList();

        if (model.DoctorId > 0)
        {
            var docResult = await doctors.GetByIdAsync(model.DoctorId, ct);
            if (docResult.Success && docResult.Data != null)
            {
                var doc = docResult.Data;
                model.DoctorName = $"Dr. {doc.FirstName} {doc.LastName}".Trim();
                model.DepartmentId = doc.DepartmentId ?? 0;
                model.DepartmentName = doc.DepartmentName;
                model.SpecializationName = doc.SpecializationName;
                model.ConsultationFee = doc.ConsultationFee;
            }

            var slots = await appointments.GetAvailableSlotsAsync(model.DoctorId, model.AppointmentDate, ct);
            model.AvailableSlots = slots.Data ?? Array.Empty<AppointmentSlot>();
        }
    }
}
