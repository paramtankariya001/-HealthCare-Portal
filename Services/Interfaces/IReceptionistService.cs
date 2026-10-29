using HealthCare_Portal.Models;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Services.Interfaces;

public interface IReceptionistService
{
    Task<OperationResult<IReadOnlyList<Receptionist>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OperationResult<Receptionist>> GetByIdAsync(int receptionistId, CancellationToken cancellationToken = default);
    Task<OperationResult<Dictionary<string, int>>> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<OperationResult<int>> CreateReceptionistAsync(ReceptionistFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> UpdateReceptionistAsync(ReceptionistFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> DeactivateReceptionistAsync(int receptionistId, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> ActivateReceptionistAsync(int receptionistId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> RegisterPatientAsync(RegisterPatientViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Patient>>> SearchPatientsAsync(string? search, CancellationToken cancellationToken = default);
    Task<OperationResult<Patient>> GetPatientByIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Appointment>>> GetAppointmentsAsync(DateTime? date, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> CheckInPatientAsync(long appointmentId, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> UpdateAppointmentStatusAsync(long appointmentId, string newStatus, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Appointment>>> GetQueueAsync(CancellationToken cancellationToken = default);
    Task<OperationResult<long>> BookAppointmentAsync(int? receptionistId, AppointmentBookingViewModel request, CancellationToken cancellationToken = default);
}
