using HealthCare_Portal.Models;
namespace HealthCare_Portal.Repositories.Interfaces;
public interface IPatientRepository { Task<Patient?> GetByIdAsync(int id, CancellationToken cancellationToken = default); Task<Patient?> GetProfileAsync(int userId, CancellationToken cancellationToken = default); Task<IReadOnlyList<Patient>> SearchAsync(string? search, CancellationToken cancellationToken = default); }
