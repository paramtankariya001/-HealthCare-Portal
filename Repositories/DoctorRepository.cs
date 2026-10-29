using System.Data;
using Microsoft.Data.SqlClient;
using HealthCare_Portal.Data;
using HealthCare_Portal.Helpers;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;

namespace HealthCare_Portal.Repositories;

public sealed class DoctorRepository(DbHelper db, DbConnectionFactory connectionFactory) : IDoctorRepository
{
    public async Task<IReadOnlyList<Doctor>> GetAllAsync(CancellationToken ct = default) =>
        (await db.ExecuteStoredProcedureAsync("dbo.usp_Doctor_GetAll", cancellationToken: ct)).Select(Map).ToList();

    public async Task<Doctor?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var r = await db.ExecuteStoredProcedureAsync("dbo.usp_Doctor_GetById", new[] { DbHelper.Parameter("@DoctorId", id) }, ct);
        return r.Count == 0 ? null : Map(r[0]);
    }

    public async Task<IReadOnlyList<Doctor>> SearchAsync(string? search, int? departmentId = null, int? specializationId = null, CancellationToken ct = default)
    {
        var r = await db.ExecuteStoredProcedureAsync("dbo.usp_Doctor_Search", new[] {
            DbHelper.Parameter("@Search", search),
            DbHelper.Parameter("@DepartmentId", departmentId),
            DbHelper.Parameter("@SpecializationId", specializationId)
        }, ct);
        return r.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<Doctor>> GetAdminDoctorsAsync(string? search = null, int? departmentId = null, string? status = null, CancellationToken ct = default)
    {
        var r = await db.ExecuteStoredProcedureAsync("dbo.usp_Admin_GetDoctors", new[] {
            DbHelper.Parameter("@Search", search),
            DbHelper.Parameter("@DepartmentId", departmentId),
            DbHelper.Parameter("@Status", status)
        }, ct);
        return r.Select(Map).ToList();
    }

    public async Task<int> CreateAsync(Doctor doctor, string? password = null, CancellationToken ct = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        try
        {
            await using var checkCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users WHERE Email = @Email", connection, (SqlTransaction)tx);
            checkCmd.Parameters.AddWithValue("@Email", doctor.Email.Trim());
            var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(ct));
            if (count > 0)
                throw new InvalidOperationException("An account with this email address already exists.");

            await using var docEmailCheck = new SqlCommand("SELECT COUNT(*) FROM dbo.Doctors WHERE Email = @Email", connection, (SqlTransaction)tx);
            docEmailCheck.Parameters.AddWithValue("@Email", doctor.Email.Trim());
            if (Convert.ToInt32(await docEmailCheck.ExecuteScalarAsync(ct)) > 0)
                throw new InvalidOperationException("A doctor with this email address already exists.");

            if (string.IsNullOrWhiteSpace(doctor.DoctorCode))
            {
                await using var codeCmd = new SqlCommand("SELECT ISNULL(MAX(DoctorId), 0) + 1 FROM dbo.Doctors", connection, (SqlTransaction)tx);
                var nextNum = Convert.ToInt32(await codeCmd.ExecuteScalarAsync(ct));
                doctor.DoctorCode = $"DR{nextNum:D4}";
            }

            var username = doctor.DoctorCode.ToLowerInvariant();
            await using var ucheckCmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users WHERE Username = @Username", connection, (SqlTransaction)tx);
            ucheckCmd.Parameters.AddWithValue("@Username", username);
            if (Convert.ToInt32(await ucheckCmd.ExecuteScalarAsync(ct)) > 0)
            {
                username = $"{username}_{Guid.NewGuid():N}"[..20];
            }

            await using var roleCmd = new SqlCommand("SELECT RoleId FROM dbo.Roles WHERE RoleName = N'Doctor' AND IsActive = 1", connection, (SqlTransaction)tx);
            var roleObj = await roleCmd.ExecuteScalarAsync(ct);
            if (roleObj is null or DBNull)
                throw new InvalidOperationException("Doctor role is not configured.");

            var hash = PasswordHelper.Hash(string.IsNullOrWhiteSpace(password) ? "Password@123" : password);
            await using var userCmd = new SqlCommand(
                "INSERT INTO dbo.Users (RoleId, Username, Email, PasswordHash, IsActive) VALUES (@RoleId, @Username, @Email, @PasswordHash, 1); SELECT CONVERT(INT, SCOPE_IDENTITY());",
                connection, (SqlTransaction)tx);
            userCmd.Parameters.AddWithValue("@RoleId", Convert.ToInt32(roleObj));
            userCmd.Parameters.AddWithValue("@Username", username);
            userCmd.Parameters.AddWithValue("@Email", doctor.Email.Trim());
            userCmd.Parameters.AddWithValue("@PasswordHash", hash);
            var userId = Convert.ToInt32(await userCmd.ExecuteScalarAsync(ct));

            await using var createCmd = new SqlCommand("dbo.usp_Doctor_Create", connection, (SqlTransaction)tx) { CommandType = CommandType.StoredProcedure };
            createCmd.Parameters.AddWithValue("@UserId", userId);
            createCmd.Parameters.AddWithValue("@DoctorCode", doctor.DoctorCode);
            createCmd.Parameters.AddWithValue("@FirstName", doctor.FirstName.Trim());
            createCmd.Parameters.AddWithValue("@LastName", (object?)doctor.LastName?.Trim() ?? DBNull.Value);
            createCmd.Parameters.AddWithValue("@Gender", doctor.Gender);
            createCmd.Parameters.AddWithValue("@DateOfBirth", (object?)doctor.DateOfBirth?.Date ?? DBNull.Value);
            createCmd.Parameters.AddWithValue("@Phone", doctor.Phone.Trim());
            createCmd.Parameters.AddWithValue("@Email", doctor.Email.Trim());
            createCmd.Parameters.AddWithValue("@Qualification", (object?)doctor.Qualification?.Trim() ?? DBNull.Value);
            createCmd.Parameters.AddWithValue("@SpecializationId", doctor.SpecializationId);
            createCmd.Parameters.AddWithValue("@ExperienceYears", doctor.ExperienceYears);
            createCmd.Parameters.AddWithValue("@ConsultationFee", doctor.ConsultationFee);
            createCmd.Parameters.AddWithValue("@AboutDoctor", (object?)doctor.AboutDoctor?.Trim() ?? DBNull.Value);

            var doctorId = Convert.ToInt32(await createCmd.ExecuteScalarAsync(ct));
            await tx.CommitAsync(ct);
            return doctorId;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Doctor doctor, CancellationToken ct = default)
    {
        await db.ExecuteNonQueryAsync("dbo.usp_Doctor_Update", new[] {
            DbHelper.Parameter("@DoctorId", doctor.DoctorId),
            DbHelper.Parameter("@FirstName", doctor.FirstName.Trim()),
            DbHelper.Parameter("@LastName", doctor.LastName?.Trim()),
            DbHelper.Parameter("@Gender", doctor.Gender),
            DbHelper.Parameter("@Phone", doctor.Phone.Trim()),
            DbHelper.Parameter("@Email", doctor.Email.Trim()),
            DbHelper.Parameter("@Qualification", doctor.Qualification?.Trim()),
            DbHelper.Parameter("@SpecializationId", doctor.SpecializationId),
            DbHelper.Parameter("@ExperienceYears", doctor.ExperienceYears),
            DbHelper.Parameter("@ConsultationFee", doctor.ConsultationFee),
            DbHelper.Parameter("@AboutDoctor", doctor.AboutDoctor?.Trim())
        }, ct);
        return true;
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct = default)
    {
        await db.ExecuteNonQueryAsync("dbo.usp_Doctor_Deactivate", new[] { DbHelper.Parameter("@DoctorId", id) }, ct);
        return true;
    }

    public async Task<bool> ActivateAsync(int id, CancellationToken ct = default)
    {
        await db.ExecuteNonQueryAsync("dbo.usp_Doctor_Activate", new[] { DbHelper.Parameter("@DoctorId", id) }, ct);
        return true;
    }

    public async Task<IReadOnlyList<Department>> GetDepartmentsAsync(CancellationToken ct = default)
    {
        var r = await db.ExecuteStoredProcedureAsync("dbo.usp_Admin_GetDepartments", cancellationToken: ct);
        return r.Select(row => new Department {
            DepartmentId = Convert.ToInt32(row["DepartmentId"]),
            DepartmentName = Convert.ToString(row["DepartmentName"]) ?? "",
            Description = row.GetValueOrDefault("Description")?.ToString(),
            IsActive = Convert.ToBoolean(row.GetValueOrDefault("IsActive") ?? true)
        }).ToList();
    }

    public async Task<IReadOnlyList<Specialization>> GetSpecializationsAsync(int? departmentId = null, CancellationToken ct = default)
    {
        var r = await db.ExecuteStoredProcedureAsync("dbo.usp_Admin_GetSpecializations", cancellationToken: ct);
        var list = r.Select(row => new Specialization {
            SpecializationId = Convert.ToInt32(row["SpecializationId"]),
            DepartmentId = Convert.ToInt32(row["DepartmentId"]),
            SpecializationName = Convert.ToString(row["SpecializationName"]) ?? "",
            Description = row.GetValueOrDefault("Description")?.ToString(),
            IsActive = Convert.ToBoolean(row.GetValueOrDefault("IsActive") ?? true)
        });

        if (departmentId.HasValue)
            list = list.Where(s => s.DepartmentId == departmentId.Value);

        return list.ToList();
    }

    public async Task<Dictionary<string, int>> GetDashboardStatsAsync(int doctorId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Doctor_GetDashboard", new[] {
            DbHelper.Parameter("@DoctorId", doctorId)
        }, ct);
        var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (rows.Count > 0)
        {
            var r = rows[0];
            dict["TodayAppointments"] = Convert.ToInt32(r.GetValueOrDefault("TodayAppointments") ?? 0);
            dict["CompletedAppointments"] = Convert.ToInt32(r.GetValueOrDefault("CompletedAppointments") ?? 0);
            dict["PatientVisits"] = Convert.ToInt32(r.GetValueOrDefault("PatientVisits") ?? 0);
        }
        return dict;
    }

    private static Doctor Map(Dictionary<string, object?> r) => new() {
        DoctorId = Convert.ToInt32(r["DoctorId"]),
        UserId = r.ContainsKey("UserId") && r["UserId"] != null ? Convert.ToInt32(r["UserId"]) : null,
        DoctorCode = Convert.ToString(r["DoctorCode"]) ?? "",
        FirstName = Convert.ToString(r["FirstName"]) ?? "",
        LastName = r.GetValueOrDefault("LastName")?.ToString(),
        Gender = Convert.ToString(r.GetValueOrDefault("Gender")) ?? "",
        DateOfBirth = r.ContainsKey("DateOfBirth") && r["DateOfBirth"] != null ? Convert.ToDateTime(r["DateOfBirth"]) : null,
        Phone = Convert.ToString(r["Phone"]) ?? "",
        Email = Convert.ToString(r["Email"]) ?? "",
        Qualification = r.GetValueOrDefault("Qualification")?.ToString(),
        SpecializationId = Convert.ToInt32(r["SpecializationId"]),
        DepartmentId = r.ContainsKey("DepartmentId") && r["DepartmentId"] != null ? Convert.ToInt32(r["DepartmentId"]) : null,
        ExperienceYears = Convert.ToInt32(r["ExperienceYears"]),
        ConsultationFee = Convert.ToDecimal(r["ConsultationFee"]),
        AboutDoctor = r.GetValueOrDefault("AboutDoctor")?.ToString(),
        SpecializationName = r.GetValueOrDefault("SpecializationName")?.ToString(),
        DepartmentName = r.GetValueOrDefault("DepartmentName")?.ToString(),
        IsActive = Convert.ToBoolean(r["IsActive"]),
        CreatedAt = r.ContainsKey("CreatedAt") && r["CreatedAt"] != null ? Convert.ToDateTime(r["CreatedAt"]) : null
    };
}
