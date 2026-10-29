USE HealthCarePortalDB;
GO
-- DEVELOPMENT ONLY: seeded PasswordHash verifies as Password@123 for sample users.
-- After seeding, run 27_EnsureDemoAdmin.sql so local Admin login is admin / admin.
-- Change the default Admin password before production deployment.
INSERT dbo.Users (RoleId, Username, Email, PasswordHash)
SELECT r.RoleId, v.Username, v.Email, N'PBKDF2-SHA256:100000:10sqKrCAsKBYV2kmTvipmA==:n5UOhTvb+jMyNl+tV9duNmKuAds0M4UwgWPp+k25Z4k='
FROM (VALUES
(N'Admin',N'admin',N'admin@healthcare.local'),(N'Receptionist',N'reception1',N'reception1@healthcare.local'),(N'Receptionist',N'reception2',N'reception2@healthcare.local'),
(N'Doctor',N'doctor1',N'doctor1@healthcare.local'),(N'Doctor',N'doctor2',N'doctor2@healthcare.local'),(N'Doctor',N'doctor3',N'doctor3@healthcare.local'),(N'Doctor',N'doctor4',N'doctor4@healthcare.local'),(N'Doctor',N'doctor5',N'doctor5@healthcare.local'),
(N'Patient',N'patient1',N'patient1@healthcare.local'),(N'Patient',N'patient2',N'patient2@healthcare.local'),(N'Patient',N'patient3',N'patient3@healthcare.local'),(N'Patient',N'patient4',N'patient4@healthcare.local'),(N'Patient',N'patient5',N'patient5@healthcare.local'),(N'Patient',N'patient6',N'patient6@healthcare.local'),(N'Patient',N'patient7',N'patient7@healthcare.local'),(N'Patient',N'patient8',N'patient8@healthcare.local'),(N'Patient',N'patient9',N'patient9@healthcare.local'),(N'Patient',N'patient10',N'patient10@healthcare.local')) v(RoleName,Username,Email)
JOIN dbo.Roles r ON r.RoleName=v.RoleName;
GO
