using HealthCare_Portal.Models;
namespace HealthCare_Portal.Repositories.Interfaces;
public interface IUserRepository { Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default); Task<User?> LoginAsync(string email, string passwordHash, CancellationToken cancellationToken = default); Task<int> RegisterPatientAsync(string username, string email, string passwordHash, string patientCode, string firstName, string? lastName, string gender, DateTime dateOfBirth, string? bloodGroup, string phone, string? address, CancellationToken cancellationToken = default); }
