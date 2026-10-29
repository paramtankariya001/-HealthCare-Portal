using HealthCare_Portal.Models;

namespace HealthCare_Portal.Repositories.Interfaces;

public interface IDoctorRepository
{
    Task<IReadOnlyList<Doctor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Doctor?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Doctor>> SearchAsync(string? search, int? departmentId = null, int? specializationId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Doctor>> GetAdminDoctorsAsync(string? search = null, int? departmentId = null, string? status = null, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(Doctor doctor, string? password = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Doctor doctor, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Department>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Specialization>> GetSpecializationsAsync(int? departmentId = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetDashboardStatsAsync(int doctorId, CancellationToken cancellationToken = default);
}
