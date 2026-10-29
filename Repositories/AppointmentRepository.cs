using HealthCare_Portal.Data;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;

namespace HealthCare_Portal.Repositories;

public sealed class AppointmentRepository(DbHelper db) : IAppointmentRepository
{
    public async Task<Appointment?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var r = await db.ExecuteStoredProcedureAsync("dbo.usp_Appointment_GetById", new[] {
            DbHelper.Parameter("@AppointmentId", id)
        }, ct);
        return r.Count == 0 ? null : Map(r[0]);
    }

    public async Task<IReadOnlyList<Appointment>> GetByPatientAsync(int id, CancellationToken ct = default) =>
        (await db.ExecuteStoredProcedureAsync("dbo.usp_Appointment_GetByPatient", new[] {
            DbHelper.Parameter("@PatientId", id)
        }, ct)).Select(Map).ToList();

    public async Task<IReadOnlyList<Appointment>> GetByDoctorAsync(int id, DateTime? from = null, DateTime? to = null, CancellationToken ct = default) =>
        (await db.ExecuteStoredProcedureAsync("dbo.usp_Appointment_GetByDoctor", new[] {
            DbHelper.Parameter("@DoctorId", id),
            DbHelper.Parameter("@FromDate", from?.Date),
            DbHelper.Parameter("@ToDate", to?.Date)
        }, ct)).Select(Map).ToList();

    public async Task<long> BookAsync(string code, int patientId, int doctorId, int departmentId, DateTime date, TimeSpan start, TimeSpan end, string? reason, CancellationToken ct = default)
    {
        var x = await db.ExecuteScalarAsync("dbo.usp_Appointment_Book", new[] {
            DbHelper.Parameter("@AppointmentCode", code),
            DbHelper.Parameter("@PatientId", patientId),
            DbHelper.Parameter("@DoctorId", doctorId),
            DbHelper.Parameter("@DepartmentId", departmentId),
            DbHelper.Parameter("@AppointmentDate", date.Date),
            DbHelper.Parameter("@StartTime", start),
            DbHelper.Parameter("@EndTime", end),
            DbHelper.Parameter("@ReasonForVisit", reason),
            DbHelper.Parameter("@BookingSource", "Patient"),
            DbHelper.Parameter("@ReceptionistId", null)
        }, ct);
        return Convert.ToInt64(x);
    }

    public async Task<bool> CheckSlotAvailabilityAsync(int doctorId, DateTime date, TimeSpan start, TimeSpan end, CancellationToken ct = default)
    {
        var x = await db.ExecuteScalarAsync("dbo.usp_Appointment_CheckSlotAvailability", new[] {
            DbHelper.Parameter("@DoctorId", doctorId),
            DbHelper.Parameter("@AppointmentDate", date.Date),
            DbHelper.Parameter("@StartTime", start),
            DbHelper.Parameter("@EndTime", end)
        }, ct);
        return Convert.ToBoolean(x);
    }

    public async Task<IReadOnlyList<AppointmentSlot>> GetAvailableSlotsAsync(int doctorId, DateTime date, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Appointment_GetAvailableSlots", new[] {
            DbHelper.Parameter("@DoctorId", doctorId),
            DbHelper.Parameter("@AppointmentDate", date.Date)
        }, ct);

        return rows.Select(r => new AppointmentSlot
        {
            SlotStart = (TimeSpan)r["SlotStart"]!,
            SlotEnd = (TimeSpan)r["SlotEnd"]!
        }).ToList();
    }

    public async Task<bool> CancelAsync(long appointmentId, CancellationToken ct = default)
    {
        await db.ExecuteNonQueryAsync("dbo.usp_Appointment_Cancel", new[] {
            DbHelper.Parameter("@AppointmentId", appointmentId)
        }, ct);
        return true;
    }

    public async Task<long> RescheduleAsync(long appointmentId, DateTime newDate, TimeSpan newStart, TimeSpan newEnd, CancellationToken ct = default)
    {
        var x = await db.ExecuteScalarAsync("dbo.usp_Appointment_Reschedule", new[] {
            DbHelper.Parameter("@AppointmentId", appointmentId),
            DbHelper.Parameter("@AppointmentDate", newDate.Date),
            DbHelper.Parameter("@StartTime", newStart),
            DbHelper.Parameter("@EndTime", newEnd)
        }, ct);
        return Convert.ToInt64(x);
    }

    public async Task<bool> UpdateStatusAsync(long appointmentId, string status, CancellationToken ct = default)
    {
        await db.ExecuteNonQueryAsync("dbo.usp_Appointment_UpdateStatus", new[] {
            DbHelper.Parameter("@AppointmentId", appointmentId),
            DbHelper.Parameter("@AppointmentStatus", status)
        }, ct);
        return true;
    }

    private static Appointment Map(Dictionary<string, object?> r)
    {
        var appt = new Appointment
        {
            AppointmentId = Convert.ToInt64(r["AppointmentId"]),
            AppointmentCode = Convert.ToString(r["AppointmentCode"]) ?? "",
            PatientId = Convert.ToInt32(r["PatientId"]),
            DoctorId = Convert.ToInt32(r["DoctorId"]),
            DepartmentId = Convert.ToInt32(r["DepartmentId"]),
            AppointmentDate = Convert.ToDateTime(r["AppointmentDate"]),
            StartTime = (TimeSpan)r["StartTime"]!,
            EndTime = (TimeSpan)r["EndTime"]!,
            ReasonForVisit = r.GetValueOrDefault("ReasonForVisit")?.ToString(),
            AppointmentStatus = Convert.ToString(r["AppointmentStatus"]) ?? "",
            BookingSource = Convert.ToString(r["BookingSource"]) ?? "",
            ConsultationFee = Convert.ToDecimal(r["ConsultationFee"])
        };

        if (r.TryGetValue("DoctorFirstName", out var docFirst) && docFirst != null && docFirst != DBNull.Value)
        {
            var docLast = r.GetValueOrDefault("DoctorLastName")?.ToString();
            appt.DoctorName = $"Dr. {docFirst} {docLast}".Trim();
        }

        if (r.TryGetValue("DoctorCode", out var docCode))
            appt.DoctorCode = docCode?.ToString();

        if (r.TryGetValue("DepartmentName", out var deptName))
            appt.DepartmentName = deptName?.ToString();

        if (r.TryGetValue("SpecializationName", out var specName))
            appt.SpecializationName = specName?.ToString();

        if (r.TryGetValue("PatientFirstName", out var pFirst) && pFirst != null && pFirst != DBNull.Value)
        {
            var pLast = r.GetValueOrDefault("PatientLastName")?.ToString();
            appt.PatientName = $"{pFirst} {pLast}".Trim();
        }

        if (r.TryGetValue("PatientCode", out var pCode))
            appt.PatientCode = pCode?.ToString();

        if (r.TryGetValue("PatientPhone", out var pPhone))
            appt.PatientPhone = pPhone?.ToString();

        if (r.TryGetValue("CreatedAt", out var createdAt) && createdAt != null && createdAt != DBNull.Value)
            appt.CreatedAt = Convert.ToDateTime(createdAt);

        return appt;
    }
}
