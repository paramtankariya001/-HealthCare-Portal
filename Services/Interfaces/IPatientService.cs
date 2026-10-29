using HealthCare_Portal.Models; namespace HealthCare_Portal.Services.Interfaces;
public interface IPatientService { Task<OperationResult<Patient>> GetByIdAsync(int id, CancellationToken cancellationToken = default); Task<OperationResult<IReadOnlyList<Patient>>> SearchAsync(string? search, CancellationToken cancellationToken = default); }
