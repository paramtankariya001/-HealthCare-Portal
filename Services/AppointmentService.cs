using Microsoft.Data.SqlClient;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;
using HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Services;

public sealed class AppointmentService(IAppointmentRepository repository, ILogger<AppointmentService> logger) : IAppointmentService
{
    public async Task<OperationResult<long>> BookAsync(int patientId, AppointmentBookingViewModel request, CancellationToken ct = default)
    {
        if (patientId <= 0)
            return OperationResult<long>.Fail("Invalid patient session.");

        if (request.DoctorId <= 0)
            return OperationResult<long>.Fail("Please select a valid doctor.");

        if (request.DepartmentId <= 0)
            return OperationResult<long>.Fail("Please select a valid department.");

        if (request.AppointmentDate.Date < DateTime.Today)
            return OperationResult<long>.Fail("Appointment date cannot be in the past.");

        if (request.StartTime <= TimeSpan.Zero)
            return OperationResult<long>.Fail("Invalid start time selected.");

        var end = request.EndTime ?? request.StartTime.Add(TimeSpan.FromMinutes(30));
        if (end <= request.StartTime)
            return OperationResult<long>.Fail("Appointment end time must be after start time.");

        if (request.AppointmentDate.Date == DateTime.Today && request.StartTime <= DateTime.Now.TimeOfDay)
            return OperationResult<long>.Fail("Cannot book an appointment for a past time today.");

        var isAvailable = await repository.CheckSlotAvailabilityAsync(request.DoctorId, request.AppointmentDate, request.StartTime, end, ct);
        if (!isAvailable)
            return OperationResult<long>.Fail("The selected appointment slot is unavailable.");

        try
        {
            var code = $"APT-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
            var appointmentId = await repository.BookAsync(code, patientId, request.DoctorId, request.DepartmentId, request.AppointmentDate, request.StartTime, end, request.Reason, ct);
            return OperationResult<long>.Ok(appointmentId, "Appointment booked successfully.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to book appointment for patient {PatientId}.", patientId);
            return OperationResult<long>.Fail(ex.Message.Contains("overlaps", StringComparison.OrdinalIgnoreCase)
                ? "The selected time overlaps an existing appointment."
                : "Unable to book the selected appointment slot.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Appointment>>> GetByPatientAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var list = await repository.GetByPatientAsync(id, ct);
            return OperationResult<IReadOnlyList<Appointment>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve appointments for patient {PatientId}.", id);
            return OperationResult<IReadOnlyList<Appointment>>.Fail("Unable to load appointments.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Appointment>>> GetByDoctorAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var list = await repository.GetByDoctorAsync(id, cancellationToken: ct);
            return OperationResult<IReadOnlyList<Appointment>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve appointments for doctor {DoctorId}.", id);
            return OperationResult<IReadOnlyList<Appointment>>.Fail("Unable to load appointments.");
        }
    }

    public async Task<OperationResult<Appointment>> GetByIdAsync(long appointmentId, CancellationToken ct = default)
    {
        try
        {
            var appt = await repository.GetByIdAsync(appointmentId, ct);
            return appt is null
                ? OperationResult<Appointment>.Fail("Appointment not found.")
                : OperationResult<Appointment>.Ok(appt);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve appointment {AppointmentId}.", appointmentId);
            return OperationResult<Appointment>.Fail("Database error while loading appointment.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<AppointmentSlot>>> GetAvailableSlotsAsync(int doctorId, DateTime date, CancellationToken ct = default)
    {
        if (doctorId <= 0)
            return OperationResult<IReadOnlyList<AppointmentSlot>>.Fail("Invalid doctor ID.");

        if (date.Date < DateTime.Today)
            return OperationResult<IReadOnlyList<AppointmentSlot>>.Ok(Array.Empty<AppointmentSlot>());

        try
        {
            var slots = await repository.GetAvailableSlotsAsync(doctorId, date, ct);
            if (date.Date == DateTime.Today)
            {
                var now = DateTime.Now.TimeOfDay;
                slots = slots.Where(s => s.SlotStart > now).ToList();
            }
            return OperationResult<IReadOnlyList<AppointmentSlot>>.Ok(slots);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve available slots for doctor {DoctorId} on date {Date}.", doctorId, date);
            return OperationResult<IReadOnlyList<AppointmentSlot>>.Fail("Unable to retrieve available slots.");
        }
    }

    public async Task<OperationResult<bool>> CancelAsync(long appointmentId, int patientId, CancellationToken ct = default)
    {
        try
        {
            var appt = await repository.GetByIdAsync(appointmentId, ct);
            if (appt is null)
                return OperationResult<bool>.Fail("Appointment not found.");

            if (appt.PatientId != patientId)
                return OperationResult<bool>.Fail("You are not authorized to cancel this appointment.");

            if (appt.AppointmentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                return OperationResult<bool>.Fail("This appointment is already cancelled.");

            if (appt.AppointmentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                appt.AppointmentStatus.Equals("No Show", StringComparison.OrdinalIgnoreCase))
                return OperationResult<bool>.Fail("Cannot cancel a completed or no-show appointment.");

            await repository.CancelAsync(appointmentId, ct);
            return OperationResult<bool>.Ok(true, "Appointment cancelled successfully.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to cancel appointment {AppointmentId}.", appointmentId);
            return OperationResult<bool>.Fail("Database error while cancelling appointment.");
        }
    }

    public async Task<OperationResult<long>> RescheduleAsync(long appointmentId, int patientId, DateTime newDate, TimeSpan newStart, TimeSpan newEnd, CancellationToken ct = default)
    {
        if (newDate.Date < DateTime.Today)
            return OperationResult<long>.Fail("New appointment date cannot be in the past.");

        if (newEnd <= newStart)
            return OperationResult<long>.Fail("Invalid time range for the appointment.");

        if (newDate.Date == DateTime.Today && newStart <= DateTime.Now.TimeOfDay)
            return OperationResult<long>.Fail("Cannot reschedule to a past time slot today.");

        try
        {
            var appt = await repository.GetByIdAsync(appointmentId, ct);
            if (appt is null)
                return OperationResult<long>.Fail("Appointment not found.");

            if (appt.PatientId != patientId)
                return OperationResult<long>.Fail("You are not authorized to reschedule this appointment.");

            if (appt.AppointmentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                return OperationResult<long>.Fail("Cannot reschedule a cancelled appointment.");

            if (appt.AppointmentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                appt.AppointmentStatus.Equals("No Show", StringComparison.OrdinalIgnoreCase))
                return OperationResult<long>.Fail("Cannot reschedule a completed or no-show appointment.");

            var newId = await repository.RescheduleAsync(appointmentId, newDate, newStart, newEnd, ct);
            return OperationResult<long>.Ok(newId, "Appointment rescheduled successfully.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to reschedule appointment {AppointmentId}.", appointmentId);
            return OperationResult<long>.Fail(ex.Message.Contains("overlaps", StringComparison.OrdinalIgnoreCase)
                ? "The selected time overlaps an existing appointment."
                : "Unable to reschedule the appointment to the selected slot.");
        }
    }

    public async Task<OperationResult<bool>> UpdateStatusAsync(long appointmentId, string status, CancellationToken ct = default)
    {
        try
        {
            var appt = await repository.GetByIdAsync(appointmentId, ct);
            if (appt is null)
                return OperationResult<bool>.Fail("Appointment not found.");

            await repository.UpdateStatusAsync(appointmentId, status, ct);
            return OperationResult<bool>.Ok(true, $"Appointment status updated to '{status}'.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to update appointment status for ID {AppointmentId}.", appointmentId);
            return OperationResult<bool>.Fail("Database error while updating appointment status.");
        }
    }
}
