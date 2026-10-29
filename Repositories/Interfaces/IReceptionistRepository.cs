using HealthCare_Portal.Models;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Repositories.Interfaces;

public interface IReceptionistRepository
{
    Task<IReadOnlyList<Receptionist>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Receptionist?> GetByIdAsync(int receptionistId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<int> CreateReceptionistAsync(Receptionist receptionist, string username, string passwordHash, CancellationToken cancellationToken = default);
    Task<bool> UpdateReceptionistAsync(Receptionist receptionist, string? username, CancellationToken cancellationToken = default);
    Task<bool> DeactivateReceptionistAsync(int receptionistId, CancellationToken cancellationToken = default);
    Task<bool> ActivateReceptionistAsync(int receptionistId, CancellationToken cancellationToken = default);
    Task<int> RegisterPatientAsync(RegisterPatientViewModel model, string passwordHash, string patientCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetAppointmentsAsync(DateTime? date, CancellationToken cancellationToken = default);
    Task<bool> CheckInPatientAsync(long appointmentId, CancellationToken cancellationToken = default);
    Task<bool> UpdateAppointmentStatusAsync(long appointmentId, string status, CancellationToken cancellationToken = default);
    Task<long> BookAppointmentAsync(string code, int patientId, int doctorId, int departmentId, DateTime date, TimeSpan start, TimeSpan end, string? reason, int? receptionistId, CancellationToken cancellationToken = default);
}
