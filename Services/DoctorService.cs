using Microsoft.Data.SqlClient;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;
using HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Services;

public sealed class DoctorService(IDoctorRepository repository, ILogger<DoctorService> logger) : IDoctorService
{
    public async Task<OperationResult<IReadOnlyList<Doctor>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var doctors = await repository.GetAllAsync(ct);
            return OperationResult<IReadOnlyList<Doctor>>.Ok(doctors);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve doctors.");
            return OperationResult<IReadOnlyList<Doctor>>.Fail("Unable to load doctors from the database.");
        }
    }

    public async Task<OperationResult<Doctor>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var doctor = await repository.GetByIdAsync(id, ct);
            return doctor is null
                ? OperationResult<Doctor>.Fail("Doctor information could not be found.")
                : OperationResult<Doctor>.Ok(doctor);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve doctor with ID {DoctorId}.", id);
            return OperationResult<Doctor>.Fail("Database error while loading doctor details.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Doctor>>> SearchAsync(string? search, int? departmentId = null, int? specializationId = null, CancellationToken ct = default)
    {
        try
        {
            var doctors = await repository.SearchAsync(search, departmentId, specializationId, ct);
            return OperationResult<IReadOnlyList<Doctor>>.Ok(doctors);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to search doctors.");
            return OperationResult<IReadOnlyList<Doctor>>.Fail("Unable to search doctors in the database.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Doctor>>> GetAdminDoctorsAsync(string? search = null, int? departmentId = null, string? status = null, CancellationToken ct = default)
    {
        try
        {
            var doctors = await repository.GetAdminDoctorsAsync(search, departmentId, status, ct);
            return OperationResult<IReadOnlyList<Doctor>>.Ok(doctors);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve admin doctor list.");
            return OperationResult<IReadOnlyList<Doctor>>.Fail("Unable to load doctor records from the database.");
        }
    }

    public async Task<OperationResult<int>> CreateDoctorAsync(DoctorFormViewModel model, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName))
            return OperationResult<int>.Fail("First name is required.");

        if (string.IsNullOrWhiteSpace(model.Email))
            return OperationResult<int>.Fail("Email address is required.");

        if (model.SpecializationId <= 0)
            return OperationResult<int>.Fail("Please select a valid specialization.");

        if (model.ConsultationFee < 0)
            return OperationResult<int>.Fail("Consultation fee cannot be negative.");

        if (model.ExperienceYears < 0)
            return OperationResult<int>.Fail("Experience years cannot be negative.");

        try
        {
            var doctor = new Doctor
            {
                DoctorCode = model.DoctorCode ?? "",
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName?.Trim(),
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Phone = model.Phone.Trim(),
                Email = model.Email.Trim(),
                Qualification = model.Qualification.Trim(),
                SpecializationId = model.SpecializationId,
                ExperienceYears = model.ExperienceYears,
                ConsultationFee = model.ConsultationFee,
                AboutDoctor = model.AboutDoctor?.Trim()
            };

            var doctorId = await repository.CreateAsync(doctor, model.Password, ct);
            return OperationResult<int>.Ok(doctorId, "Doctor created successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return OperationResult<int>.Fail(ex.Message);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to create doctor.");
            if (ex.Number is 2627 or 2601)
                return OperationResult<int>.Fail("A doctor or user with this email or doctor code already exists.");
            if (ex.Number == 547)
                return OperationResult<int>.Fail("The selected specialization or department is invalid.");
            return OperationResult<int>.Fail("A database error occurred while creating the doctor.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating doctor.");
            return OperationResult<int>.Fail("An unexpected error occurred while saving the doctor.");
        }
    }

    public async Task<OperationResult<bool>> UpdateDoctorAsync(DoctorFormViewModel model, CancellationToken ct = default)
    {
        if (model.DoctorId <= 0)
            return OperationResult<bool>.Fail("Invalid doctor ID.");

        if (string.IsNullOrWhiteSpace(model.FirstName))
            return OperationResult<bool>.Fail("First name is required.");

        if (string.IsNullOrWhiteSpace(model.Email))
            return OperationResult<bool>.Fail("Email address is required.");

        if (model.SpecializationId <= 0)
            return OperationResult<bool>.Fail("Please select a valid specialization.");

        if (model.ConsultationFee < 0)
            return OperationResult<bool>.Fail("Consultation fee cannot be negative.");

        if (model.ExperienceYears < 0)
            return OperationResult<bool>.Fail("Experience years cannot be negative.");

        try
        {
            var existing = await repository.GetByIdAsync(model.DoctorId, ct);
            if (existing is null)
                return OperationResult<bool>.Fail("Doctor record not found.");

            var doctor = new Doctor
            {
                DoctorId = model.DoctorId,
                DoctorCode = existing.DoctorCode,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName?.Trim(),
                Gender = model.Gender,
                Phone = model.Phone.Trim(),
                Email = model.Email.Trim(),
                Qualification = model.Qualification.Trim(),
                SpecializationId = model.SpecializationId,
                ExperienceYears = model.ExperienceYears,
                ConsultationFee = model.ConsultationFee,
                AboutDoctor = model.AboutDoctor?.Trim()
            };

            var success = await repository.UpdateAsync(doctor, ct);
            return success
                ? OperationResult<bool>.Ok(true, "Doctor updated successfully.")
                : OperationResult<bool>.Fail("Doctor update could not be completed.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to update doctor with ID {DoctorId}.", model.DoctorId);
            if (ex.Number is 2627 or 2601)
                return OperationResult<bool>.Fail("Another doctor or user is already using this email address.");
            if (ex.Number == 50004)
                return OperationResult<bool>.Fail("Doctor record not found.");
            return OperationResult<bool>.Fail("A database error occurred while updating the doctor.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error updating doctor with ID {DoctorId}.", model.DoctorId);
            return OperationResult<bool>.Fail("An unexpected error occurred while saving changes.");
        }
    }

    public async Task<OperationResult<bool>> DeactivateDoctorAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
            return OperationResult<bool>.Fail("Invalid doctor ID.");

        try
        {
            var existing = await repository.GetByIdAsync(id, ct);
            if (existing is null)
                return OperationResult<bool>.Fail("Doctor record not found.");

            var success = await repository.DeactivateAsync(id, ct);
            return success
                ? OperationResult<bool>.Ok(true, "Doctor deactivated successfully.")
                : OperationResult<bool>.Fail("Doctor deactivation could not be completed.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to deactivate doctor with ID {DoctorId}.", id);
            return OperationResult<bool>.Fail("A database error occurred while deactivating the doctor.");
        }
    }

    public async Task<OperationResult<bool>> ActivateDoctorAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0)
            return OperationResult<bool>.Fail("Invalid doctor ID.");

        try
        {
            var existing = await repository.GetByIdAsync(id, ct);
            if (existing is null)
                return OperationResult<bool>.Fail("Doctor record not found.");

            var success = await repository.ActivateAsync(id, ct);
            return success
                ? OperationResult<bool>.Ok(true, "Doctor activated successfully.")
                : OperationResult<bool>.Fail("Doctor activation could not be completed.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to activate doctor with ID {DoctorId}.", id);
            return OperationResult<bool>.Fail("A database error occurred while activating the doctor.");
        }
    }

    public async Task<IReadOnlyList<Department>> GetDepartmentsAsync(CancellationToken ct = default)
    {
        try { return await repository.GetDepartmentsAsync(ct); }
        catch (Exception ex) { logger.LogError(ex, "Failed to load departments."); return Array.Empty<Department>(); }
    }

    public async Task<IReadOnlyList<Specialization>> GetSpecializationsAsync(int? departmentId = null, CancellationToken ct = default)
    {
        try { return await repository.GetSpecializationsAsync(departmentId, ct); }
        catch (Exception ex) { logger.LogError(ex, "Failed to load specializations."); return Array.Empty<Specialization>(); }
    }

    public async Task<OperationResult<Dictionary<string, int>>> GetDashboardStatsAsync(int doctorId, CancellationToken ct = default)
    {
        try
        {
            var stats = await repository.GetDashboardStatsAsync(doctorId, ct);
            return OperationResult<Dictionary<string, int>>.Ok(stats);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve doctor dashboard statistics for doctor {DoctorId}.", doctorId);
            return OperationResult<Dictionary<string, int>>.Fail("Unable to load doctor dashboard statistics.");
        }
    }
}
