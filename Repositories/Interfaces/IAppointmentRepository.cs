using HealthCare_Portal.Models;

namespace HealthCare_Portal.Repositories.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetByPatientAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetByDoctorAsync(int id, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<long> BookAsync(string code, int patientId, int doctorId, int departmentId, DateTime date, TimeSpan start, TimeSpan end, string? reason, CancellationToken cancellationToken = default);
    Task<bool> CheckSlotAvailabilityAsync(int doctorId, DateTime date, TimeSpan start, TimeSpan end, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppointmentSlot>> GetAvailableSlotsAsync(int doctorId, DateTime date, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(long appointmentId, CancellationToken cancellationToken = default);
    Task<long> RescheduleAsync(long appointmentId, DateTime newDate, TimeSpan newStart, TimeSpan newEnd, CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(long appointmentId, string status, CancellationToken cancellationToken = default);
}
