using HealthCare_Portal.Models; namespace HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;
public interface IUserService { Task<OperationResult<User>> LoginAsync(string email, string password, CancellationToken cancellationToken = default); Task<OperationResult<int>> RegisterPatientAsync(RegisterPatientViewModel request, CancellationToken cancellationToken = default); }
