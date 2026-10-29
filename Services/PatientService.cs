using HealthCare_Portal.Models; using HealthCare_Portal.Repositories.Interfaces; using HealthCare_Portal.Services.Interfaces;
namespace HealthCare_Portal.Services;
public sealed class PatientService(IPatientRepository repository) : IPatientService
{
    public async Task<OperationResult<Patient>> GetByIdAsync(int id, CancellationToken ct = default) { if (id <= 0) return OperationResult<Patient>.Fail("Invalid patient."); var x = await repository.GetByIdAsync(id, ct); return x is null ? OperationResult<Patient>.Fail("Patient information could not be found.") : OperationResult<Patient>.Ok(x); }
    public async Task<OperationResult<IReadOnlyList<Patient>>> SearchAsync(string? search, CancellationToken ct = default) => OperationResult<IReadOnlyList<Patient>>.Ok(await repository.SearchAsync(search, ct));
}
