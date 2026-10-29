using Microsoft.AspNetCore.Http;

namespace HealthCare_Portal.Helpers;
public sealed class SessionHelper(IHttpContextAccessor accessor)
{
    private ISession Session => accessor.HttpContext?.Session ?? throw new InvalidOperationException("HTTP session is unavailable.");
    public void SetUserSession(int userId, int roleId, string roleName, string username, int? patientId = null, int? doctorId = null, int? receptionistId = null)
    {
        Session.SetInt32("UserId", userId); Session.SetInt32("RoleId", roleId); Session.SetString("RoleName", roleName); Session.SetString("Username", username);
        SetNullable("PatientId", patientId); SetNullable("DoctorId", doctorId); SetNullable("ReceptionistId", receptionistId);
    }
    public int? GetUserId() => Session.GetInt32("UserId"); public int? GetRoleId() => Session.GetInt32("RoleId");
    public string? GetRoleName() => Session.GetString("RoleName"); public string? GetUsername() => Session.GetString("Username");
    public int? GetPatientId() => Session.GetInt32("PatientId"); public int? GetDoctorId() => Session.GetInt32("DoctorId"); public int? GetReceptionistId() => Session.GetInt32("ReceptionistId");
    public void ClearSession() => Session.Clear();
    private void SetNullable(string key, int? value) { if (value.HasValue) Session.SetInt32(key, value.Value); else Session.Remove(key); }
}
