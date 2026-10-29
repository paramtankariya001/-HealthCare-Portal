namespace HealthCare_Portal.Models;

public sealed class AppointmentSlot
{
    public TimeSpan SlotStart { get; set; }
    public TimeSpan SlotEnd { get; set; }
    public string FormattedSlot => $"{DateTime.Today.Add(SlotStart):hh\\:mm tt} - {DateTime.Today.Add(SlotEnd):hh\\:mm tt}";
}
