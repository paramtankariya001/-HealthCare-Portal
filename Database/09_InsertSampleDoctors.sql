USE HealthCarePortalDB;
GO
INSERT dbo.Doctors (UserId,DoctorCode,FirstName,LastName,Gender,Phone,Email,Qualification,SpecializationId,ExperienceYears,ConsultationFee,AboutDoctor)
SELECT u.UserId,v.DoctorCode,v.FirstName,v.LastName,v.Gender,v.Phone,u.Email,v.Qualification,s.SpecializationId,v.ExperienceYears,v.Fee,v.AboutDoctor
FROM (VALUES
(N'doctor1',N'DR0001',N'Neel',N'Bose',N'Male',N'9100000001',N'MBBS MD',N'Clinical Cardiology',12,800,N'Cardiac evaluation'),(N'doctor2',N'DR0002',N'Maya',N'Iyer',N'Female',N'9100000002',N'MBBS DM',N'Neurology',15,950,N'Neurology specialist'),(N'doctor3',N'DR0003',N'Aditya',N'Kale',N'Male',N'9100000003',N'MBBS MS',N'Joint Replacement',10,750,N'Orthopedic surgeon'),(N'doctor4',N'DR0004',N'Tara',N'Joseph',N'Female',N'9100000004',N'MBBS MD',N'Internal Medicine',8,600,N'Primary care physician'),(N'doctor5',N'DR0005',N'Farhan',N'Khan',N'Male',N'9100000005',N'BDS MDS',N'General Dentistry',11,500,N'Dental care specialist')) v(Username,DoctorCode,FirstName,LastName,Gender,Phone,Qualification,SpecializationName,ExperienceYears,Fee,AboutDoctor)
JOIN dbo.Users u ON u.Username=v.Username JOIN dbo.Specializations s ON s.SpecializationName=v.SpecializationName;
INSERT dbo.Receptionists (UserId,EmployeeCode,FirstName,LastName,Gender,Phone,Email,JoiningDate)
SELECT UserId,EmployeeCode,FirstName,LastName,Gender,Phone,Email,JoiningDate FROM (VALUES
(N'reception1',N'EMP0001',N'Kiran',N'Patil',N'Female',N'9200000001',N'2022-01-10'),(N'reception2',N'EMP0002',N'Manav',N'Shah',N'Male',N'9200000002',N'2023-06-15')) v(Username,EmployeeCode,FirstName,LastName,Gender,Phone,JoiningDate) JOIN dbo.Users u ON u.Username=v.Username;
GO
