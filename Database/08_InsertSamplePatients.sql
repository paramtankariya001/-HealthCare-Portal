USE HealthCarePortalDB;
GO
INSERT dbo.Patients (UserId,PatientCode,FirstName,LastName,Gender,DateOfBirth,BloodGroup,Phone,Email,Address)
SELECT u.UserId, v.PatientCode,v.FirstName,v.LastName,v.Gender,v.DateOfBirth,v.BloodGroup,v.Phone,u.Email,v.Address
FROM (VALUES
(N'patient1',N'P0001',N'Aarav',N'Sharma',N'Male','1990-04-12',N'O+',N'9000000001',N'Pune'),(N'patient2',N'P0002',N'Ananya',N'Patel',N'Female','1994-07-22',N'A+',N'9000000002',N'Mumbai'),(N'patient3',N'P0003',N'Rohan',N'Mehta',N'Male','1986-01-18',N'B+',N'9000000003',N'Nashik'),(N'patient4',N'P0004',N'Sneha',N'Rao',N'Female','1998-11-02',N'AB+',N'9000000004',N'Pune'),(N'patient5',N'P0005',N'Vikram',N'Joshi',N'Male','1979-03-09',N'O-',N'9000000005',N'Nagpur'),(N'patient6',N'P0006',N'Isha',N'Kulkarni',N'Female','2001-06-30',N'B-',N'9000000006',N'Pune'),(N'patient7',N'P0007',N'Kabir',N'Desai',N'Male','1992-09-15',N'A-',N'9000000007',N'Mumbai'),(N'patient8',N'P0008',N'Neha',N'Singh',N'Female','1988-12-25',N'O+',N'9000000008',N'Pune'),(N'patient9',N'P0009',N'Arjun',N'Nair',N'Male','1996-02-14',N'AB-',N'9000000009',N'Kolhapur'),(N'patient10',N'P0010',N'Priya',N'Khan',N'Female','1983-08-05',N'A+',N'9000000010',N'Pune')) v(Username,PatientCode,FirstName,LastName,Gender,DateOfBirth,BloodGroup,Phone,Address)
JOIN dbo.Users u ON u.Username=v.Username;
GO
