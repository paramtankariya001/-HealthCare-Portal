using HealthCare_Portal.Models;

namespace HealthCare_Portal.ViewModels;

public sealed class PatientDashboardViewModel
{
    public Patient Patient { get; init; } = new();
    public IReadOnlyList<Appointment> Appointments { get; init; } = Array.Empty<Appointment>();
    public IReadOnlyList<MedicalRecord> MedicalRecords { get; init; } = Array.Empty<MedicalRecord>();
    public IReadOnlyList<Prescription> Prescriptions { get; init; } = Array.Empty<Prescription>();
    public IReadOnlyList<Bill> Bills { get; init; } = Array.Empty<Bill>();
    public IReadOnlyList<Notification> Notifications { get; init; } = Array.Empty<Notification>();
    public Appointment? UpcomingAppointment => Appointments.Where(IsUpcomingAppointment).OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime).FirstOrDefault();
    public int UpcomingAppointmentCount => Appointments.Count(IsUpcomingAppointment);
    public int MedicalRecordCount => MedicalRecords.Count;
    public int ActivePrescriptionCount => Prescriptions.Count(IsActivePrescription);
    public decimal PendingBillBalance => Bills.Where(b => b.PaymentStatus is not ("Paid" or "Refunded") && b.BalanceAmount > 0).Sum(b => b.BalanceAmount);
    public int UnreadNotificationCount => Notifications.Count(n => !n.IsRead);
    public MedicalRecord? LatestMedicalRecord => MedicalRecords.OrderByDescending(r => r.RecordDate).FirstOrDefault();
    public DateTime? NextFollowUpDate => UpcomingAppointment?.AppointmentDate;

    public static bool IsUpcomingAppointment(Appointment appointment)
    {
        if (appointment.AppointmentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) || appointment.AppointmentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)) return false;
        return appointment.AppointmentDate.Date > DateTime.Today || appointment.AppointmentDate.Date == DateTime.Today && appointment.EndTime >= DateTime.Now.TimeOfDay;
    }

    // The current schema has no prescription status/start/end columns. A null follow-up date is undated; a dated prescription remains current through follow-up.
    public static bool IsActivePrescription(Prescription prescription) => !prescription.FollowUpDate.HasValue || prescription.FollowUpDate.Value.Date >= DateTime.Today;
}
