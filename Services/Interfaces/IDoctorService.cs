using HealthCare_Portal.Models;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Services.Interfaces;

public interface IDoctorService
{
    Task<OperationResult<IReadOnlyList<Doctor>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OperationResult<Doctor>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Doctor>>> SearchAsync(string? search, int? departmentId = null, int? specializationId = null, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Doctor>>> GetAdminDoctorsAsync(string? search = null, int? departmentId = null, string? status = null, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> CreateDoctorAsync(DoctorFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> UpdateDoctorAsync(DoctorFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> DeactivateDoctorAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> ActivateDoctorAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Department>> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Specialization>> GetSpecializationsAsync(int? departmentId = null, CancellationToken cancellationToken = default);
    Task<OperationResult<Dictionary<string, int>>> GetDashboardStatsAsync(int doctorId, CancellationToken cancellationToken = default);
}
