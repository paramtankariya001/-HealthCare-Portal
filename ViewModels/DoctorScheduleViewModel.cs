using System.ComponentModel.DataAnnotations;
namespace HealthCare_Portal.ViewModels;
public sealed class DoctorScheduleViewModel { [Range(1,7)] public byte DayOfWeek { get; set; } [Required] public TimeSpan StartTime { get; set; } [Required] public TimeSpan EndTime { get; set; } public TimeSpan? BreakStart { get; set; } public TimeSpan? BreakEnd { get; set; } [Range(1,480)] public int SlotDurationMinutes { get; set; } = 30; }
