using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HealthCare_Portal.Filters;
using HealthCare_Portal.Helpers;
using HealthCare_Portal.Models;
using HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Controllers;

[AuthorizeRole("Admin")]
public class AdminController(
    SessionHelper session,
    IDoctorService doctorService,
    IReceptionistService receptionistService,
    IReportService reports,
    IBillingService billing,
    INotificationService notifications) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["Doctors"] = (await doctorService.GetAdminDoctorsAsync()).Data;
        return View();
    }

    public IActionResult Dashboard() => View();
    public IActionResult Profile() => View("Profile");
    public IActionResult Patients(string page = "Index") => View($"Patients/{page}");

    [HttpGet]
    public async Task<IActionResult> Doctors(string page = "Index", string? search = null, int? departmentId = null, string? status = null, int? id = null)
    {
        if (page.Equals("Create", StringComparison.OrdinalIgnoreCase))
            return await CreateDoctor();

        if (page.Equals("Edit", StringComparison.OrdinalIgnoreCase) && id.HasValue)
            return await EditDoctor(id.Value);

        if (page.Equals("Details", StringComparison.OrdinalIgnoreCase) && id.HasValue)
            return await DoctorDetails(id.Value);

        var doctorsResult = await doctorService.GetAdminDoctorsAsync(search, departmentId, status);
        var departments = await doctorService.GetDepartmentsAsync();

        var viewModel = new DoctorListViewModel
        {
            Doctors = doctorsResult.Data ?? Array.Empty<Doctor>(),
            Search = search,
            DepartmentId = departmentId,
            Status = status ?? "All",
            DepartmentList = departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = d.DepartmentName,
                Selected = departmentId == d.DepartmentId
            }).ToList()
        };

        return View("Doctors/Index", viewModel);
    }

    [HttpGet("Admin/Doctors/Create")]
    public async Task<IActionResult> CreateDoctor()
    {
        var model = new DoctorFormViewModel();
        await PopulateDropdownsAsync(model);
        return View("Doctors/Create", model);
    }

    [HttpPost("Admin/Doctors/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDoctor(DoctorFormViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View("Doctors/Create", model);
        }

        var result = await doctorService.CreateDoctorAsync(model, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await PopulateDropdownsAsync(model);
            return View("Doctors/Create", model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Doctors));
    }

    [HttpGet("Admin/Doctors/Edit/{id:int}")]
    public async Task<IActionResult> EditDoctor(int id, CancellationToken ct = default)
    {
        var result = await doctorService.GetByIdAsync(id, ct);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = "Doctor not found.";
            return RedirectToAction(nameof(Doctors));
        }

        var doctor = result.Data;
        var model = new DoctorFormViewModel
        {
            DoctorId = doctor.DoctorId,
            DoctorCode = doctor.DoctorCode,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            Gender = doctor.Gender,
            DateOfBirth = doctor.DateOfBirth,
            Phone = doctor.Phone,
            Email = doctor.Email,
            Qualification = doctor.Qualification ?? "",
            DepartmentId = doctor.DepartmentId ?? 0,
            SpecializationId = doctor.SpecializationId,
            ExperienceYears = doctor.ExperienceYears,
            ConsultationFee = doctor.ConsultationFee,
            AboutDoctor = doctor.AboutDoctor,
            IsActive = doctor.IsActive
        };

        await PopulateDropdownsAsync(model, doctor.DepartmentId);
        return View("Doctors/Edit", model);
    }

    [HttpPost("Admin/Doctors/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDoctor(int id, DoctorFormViewModel model, CancellationToken ct = default)
    {
        model.DoctorId = id;
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model, model.DepartmentId);
            return View("Doctors/Edit", model);
        }

        var result = await doctorService.UpdateDoctorAsync(model, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await PopulateDropdownsAsync(model, model.DepartmentId);
            return View("Doctors/Edit", model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Doctors));
    }

    [HttpGet("Admin/Doctors/Details/{id:int}")]
    public async Task<IActionResult> DoctorDetails(int id, CancellationToken ct = default)
    {
        var result = await doctorService.GetByIdAsync(id, ct);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = "Doctor not found.";
            return RedirectToAction(nameof(Doctors));
        }

        return View("Doctors/Details", result.Data);
    }

    [HttpPost("Admin/Doctors/Deactivate/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateDoctor(int id, CancellationToken ct = default)
    {
        var result = await doctorService.DeactivateDoctorAsync(id, ct);
        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Doctors));
    }

    [HttpPost("Admin/Doctors/Activate/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateDoctor(int id, CancellationToken ct = default)
    {
        var result = await doctorService.ActivateDoctorAsync(id, ct);
        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Doctors));
    }

    [HttpGet("Admin/Doctors/GetSpecializations")]
    public async Task<IActionResult> GetSpecializations(int? departmentId, CancellationToken ct = default)
    {
        var specializations = await doctorService.GetSpecializationsAsync(departmentId, ct);
        var items = specializations.Select(s => new { value = s.SpecializationId, text = s.SpecializationName });
        return Json(items);
    }

    private async Task PopulateDropdownsAsync(DoctorFormViewModel model, int? departmentId = null)
    {
        var departments = await doctorService.GetDepartmentsAsync();
        model.DepartmentList = departments.Select(d => new SelectListItem
        {
            Value = d.DepartmentId.ToString(),
            Text = d.DepartmentName,
            Selected = model.DepartmentId == d.DepartmentId
        }).ToList();

        var selectedDeptId = departmentId ?? (model.DepartmentId > 0 ? model.DepartmentId : (int?)null);
        var specializations = await doctorService.GetSpecializationsAsync(selectedDeptId);
        model.SpecializationList = specializations.Select(s => new SelectListItem
        {
            Value = s.SpecializationId.ToString(),
            Text = s.SpecializationName,
            Selected = model.SpecializationId == s.SpecializationId
        }).ToList();
    }

    [HttpGet]
    public async Task<IActionResult> Receptionists(string page = "Index", CancellationToken ct = default)
    {
        if (page.Equals("Create", StringComparison.OrdinalIgnoreCase))
            return await CreateReceptionist();
        var result = await receptionistService.GetAllAsync(ct);
        if (!result.Success)
            TempData["ErrorMessage"] = result.Message;
        return View("Receptionists/Index", result.Data ?? Array.Empty<Receptionist>());
    }

    [HttpGet("Admin/Receptionists/Create")]
    public Task<IActionResult> CreateReceptionist() => Task.FromResult<IActionResult>(View("Receptionists/Create", new ReceptionistFormViewModel()));

    [HttpPost("Admin/Receptionists/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReceptionist(ReceptionistFormViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return View("Receptionists/Create", model);

        var result = await receptionistService.CreateReceptionistAsync(model, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View("Receptionists/Create", model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Receptionists));
    }

    [HttpGet("Admin/Receptionists/Edit/{id:int}")]
    public async Task<IActionResult> EditReceptionist(int id, CancellationToken ct = default)
    {
        var result = await receptionistService.GetByIdAsync(id, ct);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Receptionists));
        }
        return View("Receptionists/Edit", ToReceptionistForm(result.Data));
    }

    [HttpPost("Admin/Receptionists/Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditReceptionist(int id, ReceptionistFormViewModel model, CancellationToken ct = default)
    {
        model.ReceptionistId = id;
        if (!ModelState.IsValid)
            return View("Receptionists/Edit", model);
        var result = await receptionistService.UpdateReceptionistAsync(model, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View("Receptionists/Edit", model);
        }
        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Receptionists));
    }

    [HttpGet("Admin/Receptionists/Details/{id:int}")]
    public async Task<IActionResult> ReceptionistDetails(int id, CancellationToken ct = default)
    {
        var result = await receptionistService.GetByIdAsync(id, ct);
        if (!result.Success || result.Data is null)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Receptionists));
        }
        return View("Receptionists/Details", result.Data);
    }

    [HttpPost("Admin/Receptionists/Deactivate/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateReceptionist(int id, CancellationToken ct = default)
    {
        var result = await receptionistService.DeactivateReceptionistAsync(id, ct);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Receptionists));
    }

    [HttpPost("Admin/Receptionists/Activate/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateReceptionist(int id, CancellationToken ct = default)
    {
        var result = await receptionistService.ActivateReceptionistAsync(id, ct);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Receptionists));
    }

    private static ReceptionistFormViewModel ToReceptionistForm(Receptionist receptionist) => new()
    {
        ReceptionistId = receptionist.ReceptionistId,
        EmployeeCode = receptionist.EmployeeCode,
        FirstName = receptionist.FirstName,
        LastName = receptionist.LastName,
        Gender = receptionist.Gender,
        Phone = receptionist.Phone,
        Email = receptionist.Email,
        Username = receptionist.Username,
        JoiningDate = receptionist.JoiningDate,
        IsActive = receptionist.IsActive
    };
    public IActionResult Departments(string page = "Index") => View($"Departments/{page}");
    public IActionResult Specializations(string page = "Index") => View($"Specializations/{page}");
    public IActionResult Schedules(string page = "Index") => View($"Schedules/{page}");
    public IActionResult Appointments(string page = "Index") => View($"Appointments/{page}");
    public IActionResult MedicalRecords(string page = "Index") => View($"MedicalRecords/{page}");
    public IActionResult Prescriptions(string page = "Index") => View($"Prescriptions/{page}");
    public IActionResult MedicalTests(string page = "Index") => View($"MedicalTests/{page}");
    public async Task<IActionResult> Billing(string page = "Index", CancellationToken ct = default)
    {
        var result = await billing.GetAllAsync(ct);
        var bills = result.Data ?? Array.Empty<Bill>();
        ViewData["Bills"] = bills;
        return View($"Billing/{page}", bills);
    }

    public async Task<IActionResult> Payments(string page = "Index", CancellationToken ct = default)
    {
        var result = await billing.GetAllPaymentsAsync(ct);
        var payments = result.Data ?? Array.Empty<Payment>();
        ViewData["Payments"] = payments;
        return View($"Payments/{page}", payments);
    }

    public async Task<IActionResult> Invoices(string page = "Index", CancellationToken ct = default)
    {
        var result = await billing.GetAllAsync(ct);
        var bills = result.Data ?? Array.Empty<Bill>();
        ViewData["Bills"] = bills;
        return View($"Invoices/{page}", bills);
    }

    [HttpGet]
    public async Task<IActionResult> Reports(string page = "Appointments", DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
        {
            TempData["ErrorMessage"] = "The report start date cannot be after the end date.";
            return RedirectToAction(nameof(Reports), new { page });
        }

        ViewData["CurrentReport"] = page;
        ViewData["FromDate"] = fromDate;
        ViewData["ToDate"] = toDate;

        if (page.Equals("Appointments", StringComparison.OrdinalIgnoreCase))
        {
            var res = await reports.GetAppointmentSummaryAsync(fromDate, toDate, ct);
            ViewData["ReportData"] = res.Data ?? Array.Empty<Dictionary<string, object?>>();
        }
        else if (page.Equals("Doctors", StringComparison.OrdinalIgnoreCase))
        {
            var res = await reports.GetDoctorPerformanceAsync(fromDate, toDate, ct);
            ViewData["ReportData"] = res.Data ?? Array.Empty<Dictionary<string, object?>>();
        }
        else if (page.Equals("Patients", StringComparison.OrdinalIgnoreCase))
        {
            var res = await reports.GetPatientSummaryAsync(fromDate, toDate, ct);
            ViewData["PatientData"] = res.Data;
        }
        else if (page.Equals("Revenue", StringComparison.OrdinalIgnoreCase))
        {
            var res = await reports.GetRevenueSummaryAsync(fromDate, toDate, ct);
            ViewData["ReportData"] = res.Data ?? Array.Empty<Dictionary<string, object?>>();
        }
        else if (page.Equals("Billing", StringComparison.OrdinalIgnoreCase))
        {
            var res = await reports.GetBillingSummaryAsync(fromDate, toDate, ct);
            ViewData["BillingData"] = res.Data;
        }

        return View($"Reports/{page}");
    }

    public async Task<IActionResult> Notifications(string page = "Index", CancellationToken ct = default)
    {
        var userId = session.GetUserId();
        if (!userId.HasValue)
            return RedirectToAction("AccessDenied", "Account");

        var result = await notifications.GetUserNotificationsAsync(userId.Value, cancellationToken: ct);
        ViewData["Notifications"] = result.Data ?? Array.Empty<Notification>();
        return View($"Notifications/{page}", result.Data ?? Array.Empty<Notification>());
    }
    public IActionResult AuditLogs(string page = "Index") => View($"AuditLogs/{page}");
    public IActionResult Settings(string page = "Index") => View($"Settings/{page}");
}
