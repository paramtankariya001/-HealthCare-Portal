using HealthCare_Portal.Data; using HealthCare_Portal.Models; using HealthCare_Portal.Repositories.Interfaces;
namespace HealthCare_Portal.Repositories;
public sealed class UserRepository(DbHelper db) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_User_GetByEmail", new[] { DbHelper.Parameter("@Email", email) }, ct);
        return rows.Count == 0 ? null : Map(rows[0]);
    }

    public async Task<User?> LoginAsync(string email, string passwordHash, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_User_Login", new[] { DbHelper.Parameter("@Email", email), DbHelper.Parameter("@PasswordHash", passwordHash) }, ct);
        return rows.Count == 0 ? null : Map(rows[0]);
    }
    public async Task<int> RegisterPatientAsync(string username, string email, string passwordHash, string patientCode, string firstName, string? lastName, string gender, DateTime dateOfBirth, string? bloodGroup, string phone, string? address, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_User_RegisterPatient", new[] {
            DbHelper.Parameter("@Username", username), DbHelper.Parameter("@Email", email), DbHelper.Parameter("@PasswordHash", passwordHash),
            DbHelper.Parameter("@PatientCode", patientCode), DbHelper.Parameter("@FirstName", firstName), DbHelper.Parameter("@LastName", lastName),
            DbHelper.Parameter("@Gender", gender), DbHelper.Parameter("@DateOfBirth", dateOfBirth.Date), DbHelper.Parameter("@BloodGroup", bloodGroup),
            DbHelper.Parameter("@Phone", phone), DbHelper.Parameter("@Address", address)
        }, ct);
        return rows.Count == 0 ? throw new InvalidOperationException("Registration did not return a user.") : Convert.ToInt32(rows[0]["UserId"]);
    }
    private static User Map(Dictionary<string, object?> r) => new() { UserId = Convert.ToInt32(r["UserId"]), RoleId = Convert.ToInt32(r["RoleId"]), Username = Convert.ToString(r["Username"]) ?? "", Email = Convert.ToString(r["Email"]) ?? "", PasswordHash = Convert.ToString(r.GetValueOrDefault("PasswordHash")) ?? "", IsActive = Convert.ToBoolean(r["IsActive"]), RoleName = r.GetValueOrDefault("RoleName")?.ToString(), PatientId = ToNullableInt(r.GetValueOrDefault("PatientId")), DoctorId = ToNullableInt(r.GetValueOrDefault("DoctorId")), ReceptionistId = ToNullableInt(r.GetValueOrDefault("ReceptionistId")) };
    private static int? ToNullableInt(object? value) => value is null or DBNull ? null : Convert.ToInt32(value);
}
