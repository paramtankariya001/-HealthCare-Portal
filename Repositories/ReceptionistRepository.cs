using HealthCare_Portal.Data;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Repositories;

public sealed class ReceptionistRepository(DbHelper db) : IReceptionistRepository
{
    public async Task<IReadOnlyList<Receptionist>> GetAllAsync(CancellationToken ct = default)
        => (await db.ExecuteStoredProcedureAsync("dbo.usp_Admin_GetReceptionists", cancellationToken: ct)).Select(Map).ToList();

    public async Task<Receptionist?> GetByIdAsync(int receptionistId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Admin_GetReceptionistById", new[] { DbHelper.Parameter("@ReceptionistId", receptionistId) }, ct);
        return rows.Count == 0 ? null : Map(rows[0]);
    }

    public async Task<Dictionary<string, int>> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Receptionist_GetDashboard", cancellationToken: ct);
        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (rows.Count > 0)
        {
            var r = rows[0];
            dict["ActivePatients"] = Convert.ToInt32(r.GetValueOrDefault("ActivePatients") ?? 0);
            dict["TodayAppointments"] = Convert.ToInt32(r.GetValueOrDefault("TodayAppointments") ?? 0);
            dict["PendingAppointments"] = Convert.ToInt32(r.GetValueOrDefault("PendingAppointments") ?? 0);
            dict["CheckedInAppointments"] = Convert.ToInt32(r.GetValueOrDefault("CheckedInAppointments") ?? 0);
        }
        return dict;
    }

    public async Task<int> CreateReceptionistAsync(Receptionist receptionist, string username, string passwordHash, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Admin_CreateReceptionist", new[]
        {
            DbHelper.Parameter("@Username", username),
            DbHelper.Parameter("@Email", receptionist.Email),
            DbHelper.Parameter("@PasswordHash", passwordHash),
            DbHelper.Parameter("@EmployeeCode", receptionist.EmployeeCode),
            DbHelper.Parameter("@FirstName", receptionist.FirstName),
            DbHelper.Parameter("@LastName", receptionist.LastName),
            DbHelper.Parameter("@Gender", receptionist.Gender),
            DbHelper.Parameter("@Phone", receptionist.Phone),
            DbHelper.Parameter("@JoiningDate", receptionist.JoiningDate)
        }, ct);
        if (rows.Count == 0 || rows[0].GetValueOrDefault("ReceptionistId") is null)
            throw new InvalidOperationException("Receptionist account was not created.");
        return Convert.ToInt32(rows[0]["ReceptionistId"]);
    }

    public async Task<bool> UpdateReceptionistAsync(Receptionist receptionist, string? username, CancellationToken ct = default)
    {
        await db.ExecuteNonQueryAsync("dbo.usp_Admin_UpdateReceptionist", new[] {
            DbHelper.Parameter("@ReceptionistId", receptionist.ReceptionistId),
            DbHelper.Parameter("@Username", username),
            DbHelper.Parameter("@Email", receptionist.Email),
            DbHelper.Parameter("@EmployeeCode", receptionist.EmployeeCode),
            DbHelper.Parameter("@FirstName", receptionist.FirstName),
            DbHelper.Parameter("@LastName", receptionist.LastName),
            DbHelper.Parameter("@Gender", receptionist.Gender),
            DbHelper.Parameter("@Phone", receptionist.Phone),
            DbHelper.Parameter("@JoiningDate", receptionist.JoiningDate)
        }, ct);
        return true;
    }

    public async Task<bool> DeactivateReceptionistAsync(int receptionistId, CancellationToken ct = default)
    {
        await db.ExecuteNonQueryAsync("dbo.usp_Admin_DeactivateReceptionist", new[] { DbHelper.Parameter("@ReceptionistId", receptionistId) }, ct);
        return true;
    }

    public async Task<bool> ActivateReceptionistAsync(int receptionistId, CancellationToken ct = default)
    {
        await db.ExecuteNonQueryAsync("dbo.usp_Admin_ActivateReceptionist", new[] { DbHelper.Parameter("@ReceptionistId", receptionistId) }, ct);
        return true;
    }

    public async Task<int> RegisterPatientAsync(RegisterPatientViewModel model, string passwordHash, string patientCode, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@Username", model.Username.Trim()),
            DbHelper.Parameter("@Email", model.Email?.Trim()),
            DbHelper.Parameter("@PasswordHash", passwordHash),
            DbHelper.Parameter("@PatientCode", patientCode),
            DbHelper.Parameter("@FirstName", model.FirstName.Trim()),
            DbHelper.Parameter("@LastName", model.LastName?.Trim()),
            DbHelper.Parameter("@Gender", model.Gender),
            DbHelper.Parameter("@DateOfBirth", model.DOB.Date),
            DbHelper.Parameter("@BloodGroup", model.BloodGroup),
            DbHelper.Parameter("@Phone", model.Phone.Trim()),
            DbHelper.Parameter("@Address", model.Address?.Trim())
        };

        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_User_RegisterPatient", parameters, ct);
        if (rows.Count > 0 && rows[0].TryGetValue("PatientId", out var pId) && pId != null)
            return Convert.ToInt32(pId);

        return 0;
    }

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsAsync(DateTime? date, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Receptionist_GetAppointments", new[] {
            DbHelper.Parameter("@Date", date?.Date)
        }, ct);

        return rows.Select(r => new Appointment
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
            ConsultationFee = Convert.ToDecimal(r["ConsultationFee"]),
            PatientName = $"{r.GetValueOrDefault("PatientFirstName")} {r.GetValueOrDefault("PatientLastName")}".Trim(),
            PatientCode = r.GetValueOrDefault("PatientCode")?.ToString(),
            PatientPhone = r.GetValueOrDefault("PatientPhone")?.ToString(),
            DoctorName = $"Dr. {r.GetValueOrDefault("DoctorFirstName")} {r.GetValueOrDefault("DoctorLastName")}".Trim(),
            DoctorCode = r.GetValueOrDefault("DoctorCode")?.ToString(),
            DepartmentName = r.GetValueOrDefault("DepartmentName")?.ToString()
        }).ToList();
    }

    public async Task<bool> CheckInPatientAsync(long appointmentId, CancellationToken ct = default)
    {
        await db.ExecuteNonQueryAsync("dbo.usp_Receptionist_CheckInPatient", new[] {
            DbHelper.Parameter("@AppointmentId", appointmentId)
        }, ct);
        return true;
    }

    public async Task<bool> UpdateAppointmentStatusAsync(long appointmentId, string status, CancellationToken ct = default)
    {
        await db.ExecuteNonQueryAsync("dbo.usp_Appointment_UpdateStatus", new[] {
            DbHelper.Parameter("@AppointmentId", appointmentId),
            DbHelper.Parameter("@AppointmentStatus", status)
        }, ct);
        return true;
    }

    public async Task<long> BookAppointmentAsync(string code, int patientId, int doctorId, int departmentId, DateTime date, TimeSpan start, TimeSpan end, string? reason, int? receptionistId, CancellationToken ct = default)
    {
        var x = await db.ExecuteScalarAsync("dbo.usp_Receptionist_BookAppointment", new[] {
            DbHelper.Parameter("@AppointmentCode", code),
            DbHelper.Parameter("@PatientId", patientId),
            DbHelper.Parameter("@DoctorId", doctorId),
            DbHelper.Parameter("@DepartmentId", departmentId),
            DbHelper.Parameter("@AppointmentDate", date.Date),
            DbHelper.Parameter("@StartTime", start),
            DbHelper.Parameter("@EndTime", end),
            DbHelper.Parameter("@ReasonForVisit", reason),
            DbHelper.Parameter("@ReceptionistId", receptionistId)
        }, ct);
        return Convert.ToInt64(x);
    }

    private static Receptionist Map(Dictionary<string, object?> r) => new()
    {
        ReceptionistId = Convert.ToInt32(r["ReceptionistId"]),
        UserId = r.GetValueOrDefault("UserId") is null ? null : Convert.ToInt32(r["UserId"]),
        Username = r.GetValueOrDefault("Username")?.ToString(),
        EmployeeCode = Convert.ToString(r["EmployeeCode"]) ?? "",
        FirstName = Convert.ToString(r["FirstName"]) ?? "",
        LastName = r.GetValueOrDefault("LastName")?.ToString(),
        Gender = Convert.ToString(r.GetValueOrDefault("Gender")) ?? "",
        Phone = Convert.ToString(r["Phone"]) ?? "",
        Email = Convert.ToString(r["Email"]) ?? "",
        JoiningDate = r.GetValueOrDefault("JoiningDate") is null ? null : Convert.ToDateTime(r["JoiningDate"]),
        IsActive = Convert.ToBoolean(r.GetValueOrDefault("IsActive") ?? true) && Convert.ToBoolean(r.GetValueOrDefault("UserIsActive") ?? true)
    };
}
