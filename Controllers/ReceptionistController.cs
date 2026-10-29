using Microsoft.AspNetCore.Mvc;
using HealthCare_Portal.Filters;
using HealthCare_Portal.Helpers;
using HealthCare_Portal.Models;
using HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Controllers;

[AuthorizeRole("Receptionist")]
public class ReceptionistController(
    SessionHelper session,
    IReceptionistService receptionist,
    IDoctorService doctors,
    IAppointmentService appointments,
    IBillingService billing,
    INotificationService notifications) : Controller
{
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var statsResult = await receptionist.GetDashboardStatsAsync(cancellationToken);
        var queueResult = await receptionist.GetQueueAsync(cancellationToken);
        var todayApptsResult = await receptionist.GetAppointmentsAsync(DateTime.Today, cancellationToken);

        ViewData["Stats"] = statsResult.Data ?? new Dictionary<string, int>();
        ViewData["Queue"] = queueResult.Data ?? Array.Empty<Appointment>();
        ViewData["TodayAppointments"] = todayApptsResult.Data ?? Array.Empty<Appointment>();

        return View();
    }

    public IActionResult Profile() => View();

    [HttpGet]
    public IActionResult PatientRegistration() => View(new RegisterPatientViewModel { DOB = DateTime.Today.AddYears(-25) });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PatientRegistration(RegisterPatientViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await receptionist.RegisterPatientAsync(model, cancellationToken);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Patients));
    }

    public async Task<IActionResult> Patients(string? search, CancellationToken cancellationToken)
    {
        var result = await receptionist.SearchPatientsAsync(search, cancellationToken);
        ViewData["Search"] = search;
        var list = result.Data ?? Array.Empty<Patient>();
        ViewData["Patients"] = list;
        return View(list);
    }

    public async Task<IActionResult> PatientDetails(int id, CancellationToken cancellationToken)
    {
        var result = await receptionist.GetPatientByIdAsync(id, cancellationToken);
        return result.Success ? View(result.Data) : NotFound();
    }

    public async Task<IActionResult> Appointments(DateTime? date, CancellationToken cancellationToken)
    {
        var result = await receptionist.GetAppointmentsAsync(date, cancellationToken);
        ViewData["SelectedDate"] = date;
        var list = result.Data ?? Array.Empty<Appointment>();
        ViewData["Appointments"] = list;
        return View(list);
    }

    public async Task<IActionResult> AppointmentManagement(DateTime? date, CancellationToken cancellationToken)
    {
        var result = await receptionist.GetAppointmentsAsync(date ?? DateTime.Today, cancellationToken);
        ViewData["SelectedDate"] = date ?? DateTime.Today;
        var list = result.Data ?? Array.Empty<Appointment>();
        ViewData["Appointments"] = list;
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> AppointmentBooking(int? patientId, int? doctorId, CancellationToken cancellationToken)
    {
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
            if (docResult.Success && docResult.Data != null)
            {
                var doc = docResult.Data;
                model.DoctorId = doc.DoctorId;
                model.DoctorName = $"Dr. {doc.FirstName} {doc.LastName}".Trim();
                model.DepartmentId = doc.DepartmentId ?? 1;
                model.ConsultationFee = doc.ConsultationFee;
            }
        }

        ViewData["PatientId"] = patientId;
        return View(model);
    }

    public IActionResult BookAppointment(int? patientId, int? doctorId) => RedirectToAction(nameof(AppointmentBooking), new { patientId, doctorId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AppointmentBooking(int patientId, AppointmentBookingViewModel model, CancellationToken cancellationToken)
    {
        if (patientId <= 0)
        {
            TempData["ErrorMessage"] = "Please select or register a patient first.";
            return View(model);
        }

        if (model.DepartmentId <= 0 && model.DoctorId > 0)
        {
            var docResult = await doctors.GetByIdAsync(model.DoctorId, cancellationToken);
            if (docResult.Success && docResult.Data != null)
                model.DepartmentId = docResult.Data.DepartmentId ?? 1;
        }

        var receptionistId = session.GetReceptionistId();
        var result = await appointments.BookAsync(patientId, model, cancellationToken);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            var allDocs = (await doctors.GetAllAsync(cancellationToken)).Data ?? Array.Empty<Doctor>();
            model.DoctorList = allDocs.Where(d => d.IsActive).ToList();
            return View(model);
        }

        TempData["SuccessMessage"] = "Appointment booked successfully on behalf of patient!";
        return RedirectToAction(nameof(Appointments));
    }

    public async Task<IActionResult> AppointmentDetails(long id, CancellationToken cancellationToken)
    {
        var result = await appointments.GetByIdAsync(id, cancellationToken);
        return result.Success ? View(result.Data) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> CheckIn(CancellationToken cancellationToken)
    {
        var queueResult = await receptionist.GetQueueAsync(cancellationToken);
        var queue = queueResult.Data ?? Array.Empty<Appointment>();
        ViewData["Queue"] = queue;
        return View(queue);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(long appointmentId, CancellationToken cancellationToken)
    {
        var result = await receptionist.CheckInPatientAsync(appointmentId, cancellationToken);
        if (!result.Success)
            TempData["ErrorMessage"] = result.Message;
        else
            TempData["SuccessMessage"] = "Patient checked in successfully!";

        return RedirectToAction(nameof(CheckIn));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(long appointmentId, string status, CancellationToken cancellationToken)
    {
        var result = await receptionist.UpdateAppointmentStatusAsync(appointmentId, status, cancellationToken);
        if (!result.Success)
            TempData["ErrorMessage"] = result.Message;
        else
            TempData["SuccessMessage"] = result.Message;

        return RedirectToAction(nameof(AppointmentManagement));
    }

    public async Task<IActionResult> DoctorAvailability(int? doctorId, DateTime? date, CancellationToken cancellationToken)
    {
        var allDocs = (await doctors.GetAllAsync(cancellationToken)).Data ?? Array.Empty<Doctor>();
        ViewData["Doctors"] = allDocs.Where(d => d.IsActive).ToList();
        ViewData["SelectedDoctorId"] = doctorId;
        ViewData["SelectedDate"] = date ?? DateTime.Today;

        if (doctorId.HasValue)
        {
            var slotsResult = await appointments.GetAvailableSlotsAsync(doctorId.Value, date ?? DateTime.Today, cancellationToken);
            ViewData["Slots"] = slotsResult.Data ?? Array.Empty<AppointmentSlot>();
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Billing(CancellationToken cancellationToken)
    {
        var result = await billing.GetAllAsync(cancellationToken);
        var bills = result.Data ?? Array.Empty<Bill>();
        ViewData["Bills"] = bills;
        ViewData["TotalBilled"] = bills.Sum(b => b.GrandTotal);
        ViewData["TotalPaid"] = bills.Sum(b => b.PaidAmount);
        ViewData["TotalOutstanding"] = bills.Sum(b => b.BalanceAmount);
        return View(bills);
    }

    [HttpGet]
    public async Task<IActionResult> CreateBill(int? patientId, long? appointmentId, CancellationToken cancellationToken)
    {
        var model = new BillViewModel
        {
            PatientId = patientId ?? 0,
            AppointmentId = appointmentId
        };

        if (patientId.HasValue && patientId.Value > 0)
        {
            var pRes = await receptionist.GetPatientByIdAsync(patientId.Value, cancellationToken);
            if (pRes.Success && pRes.Data != null)
            {
                model.PatientName = $"{pRes.Data.FirstName} {pRes.Data.LastName}".Trim();
                model.PatientCode = pRes.Data.PatientCode;
            }
        }

        if (appointmentId.HasValue && appointmentId.Value > 0)
        {
            var aRes = await appointments.GetByIdAsync(appointmentId.Value, cancellationToken);
            if (aRes.Success && aRes.Data != null)
            {
                model.PatientId = aRes.Data.PatientId;
                model.PatientName = aRes.Data.PatientName;
                model.PatientCode = aRes.Data.PatientCode;
                model.DoctorId = aRes.Data.DoctorId;
                model.DoctorName = aRes.Data.DoctorName;

                model.BillItems.Add(new BillItemViewModel
                {
                    ServiceName = "Doctor Consultation",
                    Description = $"Consultation with {aRes.Data.DoctorName}",
                    Quantity = 1,
                    UnitPrice = aRes.Data.ConsultationFee
                });
            }
        }

        if (model.BillItems.Count == 0)
        {
            model.BillItems.Add(new BillItemViewModel
            {
                ServiceName = "General Consultation",
                Quantity = 1,
                UnitPrice = 0
            });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBill(BillViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = session.GetUserId();
        var result = await billing.CreateBillAsync(model, userId, cancellationToken);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(InvoiceDetails), new { id = result.Data });
    }

    [HttpGet]
    public async Task<IActionResult> Payments(CancellationToken cancellationToken)
    {
        var result = await billing.GetAllPaymentsAsync(cancellationToken);
        var payments = result.Data ?? Array.Empty<Payment>();
        ViewData["Payments"] = payments;
        return View(payments);
    }

    [HttpGet]
    public async Task<IActionResult> Payment(long? billId, CancellationToken cancellationToken)
    {
        if (!billId.HasValue || billId.Value <= 0)
            return RedirectToAction(nameof(Billing));

        var billRes = await billing.GetByIdAsync(billId.Value, cancellationToken);
        if (!billRes.Success || billRes.Data == null)
        {
            TempData["ErrorMessage"] = "Bill not found.";
            return RedirectToAction(nameof(Billing));
        }

        var bill = billRes.Data;
        var model = new PaymentViewModel
        {
            BillId = bill.BillId,
            InvoiceNumber = bill.InvoiceNumber,
            PatientId = bill.PatientId,
            PatientName = bill.PatientName,
            TotalAmount = bill.GrandTotal,
            PaidAmount = bill.PaidAmount,
            RemainingBalance = bill.BalanceAmount,
            Amount = bill.BalanceAmount
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Payment(PaymentViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await billing.RecordPaymentAsync(model, cancellationToken);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(InvoiceDetails), new { id = model.BillId });
    }

    [HttpGet]
    public async Task<IActionResult> Invoices(CancellationToken cancellationToken)
    {
        var result = await billing.GetAllAsync(cancellationToken);
        var bills = result.Data ?? Array.Empty<Bill>();
        return View(bills);
    }

    [HttpGet]
    public async Task<IActionResult> InvoiceDetails(long? id, string? invoiceNumber, CancellationToken cancellationToken)
    {
        Bill? bill = null;
        if (id.HasValue && id.Value > 0)
        {
            var res = await billing.GetByIdAsync(id.Value, cancellationToken);
            if (res.Success) bill = res.Data;
        }
        else if (!string.IsNullOrWhiteSpace(invoiceNumber))
        {
            var res = await billing.GetByInvoiceNumberAsync(invoiceNumber, cancellationToken);
            if (res.Success) bill = res.Data;
        }

        if (bill == null)
            return NotFound();

        return View(bill);
    }

    [HttpGet]
    public async Task<IActionResult> Notifications(CancellationToken cancellationToken)
    {
        var userId = session.GetUserId();
        if (!userId.HasValue)
            return RedirectToAction("AccessDenied", "Account");

        var result = await notifications.GetUserNotificationsAsync(userId.Value, cancellationToken: cancellationToken);
        var list = result.Data ?? Array.Empty<Notification>();
        return View(list);
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
