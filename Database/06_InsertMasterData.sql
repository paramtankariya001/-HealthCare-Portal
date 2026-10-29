USE HealthCarePortalDB;
GO
INSERT dbo.Departments (DepartmentName, Description) VALUES
(N'Cardiology',N'Heart and cardiovascular care'),(N'Neurology',N'Nervous system care'),(N'Orthopedics',N'Bones and joints'),(N'Pediatrics',N'Child healthcare'),
(N'Dermatology',N'Skin care'),(N'General Medicine',N'Primary and internal medicine'),(N'Gynecology',N'Women health'),(N'ENT',N'Ear nose and throat'),(N'Dental',N'Oral healthcare');
INSERT dbo.Specializations (DepartmentId, SpecializationName, Description)
SELECT DepartmentId, v.SpecializationName, v.Description FROM dbo.Departments d JOIN (VALUES
(N'Cardiology',N'Interventional Cardiology',N'Catheter-based cardiac care'),(N'Cardiology',N'Clinical Cardiology',N'Heart disease diagnosis'),
(N'Neurology',N'Neurology',N'Brain and nerve disorders'),(N'Orthopedics',N'Joint Replacement',N'Joint and mobility surgery'),
(N'Pediatrics',N'Child Medicine',N'General pediatric care'),(N'Dermatology',N'Clinical Dermatology',N'Skin disease treatment'),
(N'General Medicine',N'Internal Medicine',N'Adult primary care'),(N'Gynecology',N'Obstetrics and Gynecology',N'Women health and maternity'),
(N'ENT',N'Otolaryngology',N'Ear nose and throat care'),(N'Dental',N'General Dentistry',N'Dental diagnosis and treatment')) v(DepartmentName,SpecializationName,Description) ON d.DepartmentName=v.DepartmentName;
GO
