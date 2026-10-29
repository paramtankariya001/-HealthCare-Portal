using HealthCare_Portal.Helpers;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;
using HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Services;

public sealed class UserService(IUserRepository repository) : IUserService
{
    public async Task<OperationResult<int>> RegisterPatientAsync(RegisterPatientViewModel request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return OperationResult<int>.Fail("Login ID and password are required.");
        if (request.DOB.Date > DateTime.Today || request.Password != request.ConfirmPassword)
            return OperationResult<int>.Fail("Registration details are invalid.");
        try
        {
            var username = request.Username.Trim();
            var email = string.IsNullOrWhiteSpace(request.Email) ? $"{username}@healthcare.local" : request.Email.Trim();
            var id = await repository.RegisterPatientAsync(username, email, PasswordHelper.Hash(request.Password),
                $"PAT-{Guid.NewGuid():N}"[..12].ToUpperInvariant(), request.FirstName.Trim(), request.LastName?.Trim(),
                request.Gender, request.DOB, request.BloodGroup, request.Phone.Trim(), request.Address?.Trim(), ct);
            return OperationResult<int>.Ok(id, "Registration completed. You can now sign in with your Login ID or email.");
        }
        catch (Microsoft.Data.SqlClient.SqlException) { return OperationResult<int>.Fail("Unable to create the account. The username or email may already exist."); }
    }

    public async Task<OperationResult<User>> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return OperationResult<User>.Fail("Login ID or email and password are required.");

        try
        {
            var candidate = await repository.GetByEmailAsync(email.Trim(), ct);
            if (candidate is null)
                return OperationResult<User>.Fail("Invalid credentials.");

            if (!candidate.IsActive)
                return OperationResult<User>.Fail("Your account is inactive. Please contact administration.");

            if (!PasswordHelper.Verify(password, candidate.PasswordHash))
                return OperationResult<User>.Fail("Invalid credentials.");

            var user = await repository.LoginAsync(candidate.Email, candidate.PasswordHash, ct);
            if (user is null)
                return OperationResult<User>.Fail("Invalid credentials.");

            user.PatientId ??= candidate.PatientId;
            user.DoctorId ??= candidate.DoctorId;
            user.ReceptionistId ??= candidate.ReceptionistId;

            return OperationResult<User>.Ok(user);
        }
        catch (Microsoft.Data.SqlClient.SqlException)
        {
            return OperationResult<User>.Fail("Unable to connect to the database. Please try again later.");
        }
        catch (Exception)
        {
            return OperationResult<User>.Fail("An unexpected error occurred. Please try again later.");
        }
    }
}
