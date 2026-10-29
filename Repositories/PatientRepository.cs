using HealthCare_Portal.Data; using HealthCare_Portal.Models; using HealthCare_Portal.Repositories.Interfaces;
namespace HealthCare_Portal.Repositories;
public sealed class PatientRepository(DbHelper db) : IPatientRepository
{
    public async Task<Patient?> GetByIdAsync(int id, CancellationToken ct = default) => First(await Rows("dbo.usp_Patient_GetById", ct, ("@PatientId", id)));
    public async Task<Patient?> GetProfileAsync(int userId, CancellationToken ct = default) => First(await Rows("dbo.usp_Patient_GetProfile", ct, ("@UserId", userId)));
    public async Task<IReadOnlyList<Patient>> SearchAsync(string? search, CancellationToken ct = default) => (await Rows("dbo.usp_Patient_Search", ct, ("@Search", search))).Select(Map).ToList();
    private Task<List<Dictionary<string, object?>>> Rows(string p, CancellationToken ct, params (string, object?)[] values) => db.ExecuteStoredProcedureAsync(p, values.Select(x => DbHelper.Parameter(x.Item1, x.Item2)), ct);
    private static Patient? First(List<Dictionary<string, object?>> rows) => rows.Count == 0 ? null : Map(rows[0]);
    private static Patient Map(Dictionary<string, object?> r) => new() { PatientId = Convert.ToInt32(r["PatientId"]), UserId = r.GetValueOrDefault("UserId") is null ? null : Convert.ToInt32(r["UserId"]), PatientCode = Convert.ToString(r["PatientCode"]) ?? "", FirstName = Convert.ToString(r["FirstName"]) ?? "", LastName = r.GetValueOrDefault("LastName")?.ToString(), Gender = Convert.ToString(r["Gender"]) ?? "", DateOfBirth = Convert.ToDateTime(r["DateOfBirth"]), Phone = Convert.ToString(r["Phone"]) ?? "", Email = r.GetValueOrDefault("Email")?.ToString(), IsActive = Convert.ToBoolean(r["IsActive"]) };
}
