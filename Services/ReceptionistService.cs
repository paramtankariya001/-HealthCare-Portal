using Microsoft.Data.SqlClient;
using HealthCare_Portal.Helpers;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;
using HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Services;

public sealed class ReceptionistService(
    IReceptionistRepository receptionistRepository,
    IPatientRepository patientRepository,
    IAppointmentRepository appointmentRepository,
    ILogger<ReceptionistService> logger) : IReceptionistService
{
    public async Task<OperationResult<IReadOnlyList<Receptionist>>> GetAllAsync(CancellationToken ct = default)
    {
        try { return OperationResult<IReadOnlyList<Receptionist>>.Ok(await receptionistRepository.GetAllAsync(ct)); }
        catch (SqlException ex) { logger.LogError(ex, "Failed to retrieve receptionist records."); return OperationResult<IReadOnlyList<Receptionist>>.Fail("Unable to load receptionist records."); }
    }

    public async Task<OperationResult<Receptionist>> GetByIdAsync(int receptionistId, CancellationToken ct = default)
    {
        try
        {
            var receptionist = await receptionistRepository.GetByIdAsync(receptionistId, ct);
            return receptionist is null ? OperationResult<Receptionist>.Fail("Receptionist record not found.") : OperationResult<Receptionist>.Ok(receptionist);
        }
        catch (SqlException ex) { logger.LogError(ex, "Failed to retrieve receptionist {ReceptionistId}.", receptionistId); return OperationResult<Receptionist>.Fail("Unable to load receptionist details."); }
    }

    public async Task<OperationResult<Dictionary<string, int>>> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        try
        {
            var stats = await receptionistRepository.GetDashboardStatsAsync(ct);
            return OperationResult<Dictionary<string, int>>.Ok(stats);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve receptionist dashboard statistics.");
            return OperationResult<Dictionary<string, int>>.Fail("Unable to load front desk statistics.");
        }
    }

    public async Task<OperationResult<int>> CreateReceptionistAsync(ReceptionistFormViewModel model, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Phone))
            return OperationResult<int>.Fail("First name, email, and phone are required.");

        try
        {
            var employeeCode = string.IsNullOrWhiteSpace(model.EmployeeCode)
                ? $"EMP{Guid.NewGuid():N}"[..10].ToUpperInvariant()
                : model.EmployeeCode.Trim();
            var username = string.IsNullOrWhiteSpace(model.Username) ? employeeCode.ToLowerInvariant() : model.Username.Trim();
            var passwordHash = PasswordHelper.Hash(string.IsNullOrWhiteSpace(model.Password) ? "Password@123" : model.Password);
            var receptionist = new Receptionist
            {
                EmployeeCode = employeeCode,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName?.Trim(),
                Gender = model.Gender,
                Phone = model.Phone.Trim(),
                Email = model.Email.Trim(),
                JoiningDate = model.JoiningDate
            };
            var id = await receptionistRepository.CreateReceptionistAsync(receptionist, username, passwordHash, ct);
            return OperationResult<int>.Ok(id, "Receptionist account created successfully.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to create receptionist account.");
            if (ex.Number is 2627 or 2601)
                return OperationResult<int>.Fail("A user with this login ID or email already exists.");
            return OperationResult<int>.Fail("Unable to create the receptionist account.");
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<int>.Fail(ex.Message);
        }
    }

    public async Task<OperationResult<bool>> UpdateReceptionistAsync(ReceptionistFormViewModel model, CancellationToken ct = default)
    {
        if (model.ReceptionistId <= 0 || string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Phone))
            return OperationResult<bool>.Fail("Receptionist ID, first name, email, and phone are required.");
        try
        {
            var existing = await receptionistRepository.GetByIdAsync(model.ReceptionistId, ct);
            if (existing is null) return OperationResult<bool>.Fail("Receptionist record not found.");
            var receptionist = new Receptionist { ReceptionistId = model.ReceptionistId, UserId = existing.UserId, EmployeeCode = string.IsNullOrWhiteSpace(model.EmployeeCode) ? existing.EmployeeCode : model.EmployeeCode.Trim(), FirstName = model.FirstName.Trim(), LastName = model.LastName?.Trim(), Gender = model.Gender, Phone = model.Phone.Trim(), Email = model.Email.Trim(), JoiningDate = model.JoiningDate };
            var username = string.IsNullOrWhiteSpace(model.Username) ? null : model.Username.Trim();
            return await receptionistRepository.UpdateReceptionistAsync(receptionist, username, ct)
                ? OperationResult<bool>.Ok(true, "Receptionist updated successfully.")
                : OperationResult<bool>.Fail("Receptionist update could not be completed.");
        }
        catch (SqlException ex) { logger.LogError(ex, "Failed to update receptionist {ReceptionistId}.", model.ReceptionistId); return OperationResult<bool>.Fail(ex.Number is 2627 or 2601 ? "The employee code, username, or email is already in use." : "Unable to update the receptionist."); }
    }

    public async Task<OperationResult<bool>> DeactivateReceptionistAsync(int id, CancellationToken ct = default) => await SetReceptionistStatusAsync(id, false, ct);
    public async Task<OperationResult<bool>> ActivateReceptionistAsync(int id, CancellationToken ct = default) => await SetReceptionistStatusAsync(id, true, ct);

    private async Task<OperationResult<bool>> SetReceptionistStatusAsync(int id, bool active, CancellationToken ct)
    {
        if (id <= 0) return OperationResult<bool>.Fail("Invalid receptionist ID.");
        try
        {
            var existing = await receptionistRepository.GetByIdAsync(id, ct);
            if (existing is null) return OperationResult<bool>.Fail("Receptionist record not found.");
            var success = active ? await receptionistRepository.ActivateReceptionistAsync(id, ct) : await receptionistRepository.DeactivateReceptionistAsync(id, ct);
            return success ? OperationResult<bool>.Ok(true, active ? "Receptionist activated successfully." : "Receptionist deactivated successfully.") : OperationResult<bool>.Fail("The status change could not be completed.");
        }
        catch (SqlException ex) { logger.LogError(ex, "Failed to change receptionist {ReceptionistId} status.", id); return OperationResult<bool>.Fail("Unable to change receptionist status."); }
    }

    public async Task<OperationResult<int>> RegisterPatientAsync(RegisterPatientViewModel model, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName))
            return OperationResult<int>.Fail("First name is required.");

        if (string.IsNullOrWhiteSpace(model.Phone))
            return OperationResult<int>.Fail("Phone number is required.");

        if (string.IsNullOrWhiteSpace(model.Username))
            return OperationResult<int>.Fail("Username is required.");

        if (string.IsNullOrWhiteSpace(model.Password))
            return OperationResult<int>.Fail("Password is required.");

        if (model.Password != model.ConfirmPassword)
            return OperationResult<int>.Fail("Passwords do not match.");

        try
        {
            if (string.IsNullOrWhiteSpace(model.Email))
                model.Email = $"{model.Username.Trim()}@healthcare.local";
            var passwordHash = PasswordHelper.Hash(model.Password);
            var patientCode = $"PT-{Guid.NewGuid():N}"[..8].ToUpperInvariant();

            var patientId = await receptionistRepository.RegisterPatientAsync(model, passwordHash, patientCode, ct);
            if (patientId <= 0)
                return OperationResult<int>.Fail("Failed to register patient.");

            return OperationResult<int>.Ok(patientId, $"Patient registered successfully with ID: {patientCode}.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to register patient with username {Username}.", model.Username);
            if (ex.Number == 2627 || ex.Number == 2601)
                return OperationResult<int>.Fail("An account with this username, email, or phone number already exists.");
            return OperationResult<int>.Fail("Unable to create the account. Please try again later.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Patient>>> SearchPatientsAsync(string? search, CancellationToken ct = default)
    {
        try
        {
            var list = await patientRepository.SearchAsync(search, ct);
            return OperationResult<IReadOnlyList<Patient>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to search patients with query '{Query}'.", search);
            return OperationResult<IReadOnlyList<Patient>>.Fail("Unable to search patients.");
        }
    }

    public async Task<OperationResult<Patient>> GetPatientByIdAsync(int patientId, CancellationToken ct = default)
    {
        try
        {
            var patient = await patientRepository.GetByIdAsync(patientId, ct);
            return patient is null
                ? OperationResult<Patient>.Fail("Patient record not found.")
                : OperationResult<Patient>.Ok(patient);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve patient {PatientId}.", patientId);
            return OperationResult<Patient>.Fail("Database error while loading patient.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Appointment>>> GetAppointmentsAsync(DateTime? date, CancellationToken ct = default)
    {
        try
        {
            var list = await receptionistRepository.GetAppointmentsAsync(date, ct);
            return OperationResult<IReadOnlyList<Appointment>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve receptionist appointments.");
            return OperationResult<IReadOnlyList<Appointment>>.Fail("Unable to load appointments.");
        }
    }

    public async Task<OperationResult<bool>> CheckInPatientAsync(long appointmentId, CancellationToken ct = default)
    {
        try
        {
            var appt = await appointmentRepository.GetByIdAsync(appointmentId, ct);
            if (appt is null)
                return OperationResult<bool>.Fail("Appointment not found.");

            if (appt.AppointmentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                return OperationResult<bool>.Fail("Cannot check in a cancelled appointment.");

            if (appt.AppointmentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                return OperationResult<bool>.Fail("This appointment has already been completed.");

            if (appt.AppointmentStatus.Equals("Checked-In", StringComparison.OrdinalIgnoreCase))
                return OperationResult<bool>.Fail("Patient is already checked in for this appointment.");

            if (appt.AppointmentStatus.Equals("In Consultation", StringComparison.OrdinalIgnoreCase))
                return OperationResult<bool>.Fail("Patient is currently in consultation.");

            if (appt.AppointmentDate.Date != DateTime.Today)
                return OperationResult<bool>.Fail("Check-in is available only on the appointment date.");

            await receptionistRepository.CheckInPatientAsync(appointmentId, ct);
            return OperationResult<bool>.Ok(true, "Patient checked in successfully.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to check in appointment {AppointmentId}.", appointmentId);
            return OperationResult<bool>.Fail("Database error while processing patient check-in.");
        }
    }

    public async Task<OperationResult<bool>> UpdateAppointmentStatusAsync(long appointmentId, string newStatus, CancellationToken ct = default)
    {
        try
        {
            var requestedStatus = newStatus?.Trim();
            var allowedStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Checked-In", "In Consultation", "Cancelled"
            };

            if (string.IsNullOrWhiteSpace(requestedStatus) || !allowedStatuses.Contains(requestedStatus))
                return OperationResult<bool>.Fail("That appointment status is not permitted from the front desk.");

            var appt = await appointmentRepository.GetByIdAsync(appointmentId, ct);
            if (appt is null)
                return OperationResult<bool>.Fail("Appointment not found.");

            if (requestedStatus.Equals("Checked-In", StringComparison.OrdinalIgnoreCase) &&
                (appt.AppointmentStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                 appt.AppointmentStatus.Equals("Confirmed", StringComparison.OrdinalIgnoreCase)))
            {
                if (appt.AppointmentDate.Date != DateTime.Today)
                    return OperationResult<bool>.Fail("Check-in is available only on the appointment date.");
            }
            else if (requestedStatus.Equals("In Consultation", StringComparison.OrdinalIgnoreCase) &&
                     !appt.AppointmentStatus.Equals("Checked-In", StringComparison.OrdinalIgnoreCase))
            {
                return OperationResult<bool>.Fail("Only checked-in patients can be moved into consultation.");
            }
            else if (requestedStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) &&
                     (appt.AppointmentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
                      appt.AppointmentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)))
            {
                return OperationResult<bool>.Fail("This appointment can no longer be cancelled.");
            }

            await receptionistRepository.UpdateAppointmentStatusAsync(appointmentId, requestedStatus, ct);
            return OperationResult<bool>.Ok(true, $"Appointment status updated to '{requestedStatus}'.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to update status for appointment {AppointmentId}.", appointmentId);
            return OperationResult<bool>.Fail("Database error while updating appointment status.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Appointment>>> GetQueueAsync(CancellationToken ct = default)
    {
        try
        {
            var allToday = await receptionistRepository.GetAppointmentsAsync(DateTime.Today, ct);
            var queue = allToday
                .Where(a => !a.AppointmentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) &&
                            !a.AppointmentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                .OrderBy(a => a.AppointmentStatus switch
                {
                    "In Consultation" => 1,
                    "Checked-In" => 2,
                    "Confirmed" => 3,
                    _ => 4
                })
                .ThenBy(a => a.StartTime)
                .ToList();

            return OperationResult<IReadOnlyList<Appointment>>.Ok(queue);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve consultation queue.");
            return OperationResult<IReadOnlyList<Appointment>>.Fail("Unable to load consultation queue.");
        }
    }

    public async Task<OperationResult<long>> BookAppointmentAsync(int? receptionistId, AppointmentBookingViewModel request, CancellationToken ct = default)
    {
        if (request.DoctorId <= 0 || request.DepartmentId <= 0 || request.AppointmentDate.Date < DateTime.Today)
            return OperationResult<long>.Fail("Invalid appointment details.");

        var end = request.EndTime ?? request.StartTime.Add(TimeSpan.FromMinutes(30));
        var isAvailable = await appointmentRepository.CheckSlotAvailabilityAsync(request.DoctorId, request.AppointmentDate, request.StartTime, end, ct);
        if (!isAvailable)
            return OperationResult<long>.Fail("The selected appointment slot is unavailable.");

        try
        {
            var code = $"APT-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
            var id = await receptionistRepository.BookAppointmentAsync(code, request.DoctorId, request.DoctorId, request.DepartmentId, request.AppointmentDate, request.StartTime, end, request.Reason, receptionistId, ct);
            return OperationResult<long>.Ok(id, "Appointment booked successfully.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to book appointment by receptionist.");
            return OperationResult<long>.Fail(ex.Message);
        }
    }
}
