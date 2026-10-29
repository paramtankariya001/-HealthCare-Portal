USE HealthCarePortalDB;
GO
INSERT dbo.Roles (RoleName, Description) VALUES
(N'Admin', N'Portal administrator'), (N'Doctor', N'Clinical practitioner'),
(N'Receptionist', N'Front desk and appointment operator'), (N'Patient', N'Patient portal user');
GO
