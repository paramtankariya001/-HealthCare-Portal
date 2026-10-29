using HealthCare_Portal.Models;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Services.Interfaces;

public interface IAppointmentService
{
    Task<OperationResult<long>> BookAsync(int patientId, AppointmentBookingViewModel request, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Appointment>>> GetByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Appointment>>> GetByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<OperationResult<Appointment>> GetByIdAsync(long appointmentId, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<AppointmentSlot>>> GetAvailableSlotsAsync(int doctorId, DateTime date, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> CancelAsync(long appointmentId, int patientId, CancellationToken cancellationToken = default);
    Task<OperationResult<long>> RescheduleAsync(long appointmentId, int patientId, DateTime newDate, TimeSpan newStart, TimeSpan newEnd, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> UpdateStatusAsync(long appointmentId, string status, CancellationToken cancellationToken = default);
}
