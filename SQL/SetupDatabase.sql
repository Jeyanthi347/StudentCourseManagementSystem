/*
IMPORTANT:
- This script creates the database only if it does not already exist.
- If you already have a StudentCourseDB with earlier Students/Courses/Enrollments tables,
  do NOT run this blindly against it. Back up your database and inspect the schema first.
- Run this script in SSMS connected to .\SQLEXPRESS.
*/
IF DB_ID(N'StudentCourseDB') IS NULL
    CREATE DATABASE StudentCourseDB;
GO
USE StudentCourseDB;
GO

IF OBJECT_ID(N'dbo.Departments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Departments
    (
        DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
        DepartmentName NVARCHAR(100) NOT NULL UNIQUE,
        Location NVARCHAR(100) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Departments_IsActive DEFAULT 1
    );
END;
GO

IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Students
    (
        StudentId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        Age INT NOT NULL CHECK (Age BETWEEN 17 AND 60),
        DepartmentId INT NOT NULL,
        AdmissionDate DATETIME2 NOT NULL CONSTRAINT DF_Students_AdmissionDate DEFAULT SYSDATETIME(),
        IsActive BIT NOT NULL CONSTRAINT DF_Students_IsActive DEFAULT 1,
        CONSTRAINT FK_Students_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Courses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Courses
    (
        CourseId INT IDENTITY(1,1) PRIMARY KEY,
        CourseName NVARCHAR(100) NOT NULL UNIQUE,
        DurationMonths INT NOT NULL CHECK (DurationMonths > 0),
        Fee DECIMAL(10,2) NOT NULL CHECK (Fee >= 0),
        DepartmentId INT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Courses_IsActive DEFAULT 1,
        CONSTRAINT FK_Courses_Departments FOREIGN KEY (DepartmentId) REFERENCES dbo.Departments(DepartmentId)
    );
END;
GO

IF OBJECT_ID(N'dbo.Enrollments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Enrollments
    (
        EnrollmentId INT IDENTITY(1,1) PRIMARY KEY,
        StudentId INT NOT NULL,
        CourseId INT NOT NULL,
        EnrolledAt DATETIME2 NOT NULL CONSTRAINT DF_Enrollments_EnrolledAt DEFAULT SYSDATETIME(),
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Enrollments_Status DEFAULT N'Enrolled',
        CONSTRAINT FK_Enrollments_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
        CONSTRAINT FK_Enrollments_Courses FOREIGN KEY (CourseId) REFERENCES dbo.Courses(CourseId),
        CONSTRAINT UQ_Enrollments_Student_Course UNIQUE (StudentId, CourseId),
        CONSTRAINT CK_Enrollments_Status CHECK (Status IN (N'Enrolled', N'Completed', N'Cancelled'))
    );
END;
GO

IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        AuditId BIGINT IDENTITY(1,1) PRIMARY KEY,
        EntityName NVARCHAR(50) NOT NULL,
        EntityId INT NOT NULL,
        Action NVARCHAR(20) NOT NULL,
        Details NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT SYSDATETIME()
    );
END;
GO

/* Insert sample rows only when the relevant table is empty. */
IF NOT EXISTS (SELECT 1 FROM dbo.Departments)
BEGIN
    INSERT INTO dbo.Departments (DepartmentName, Location)
    VALUES (N'Computer Science', N'Block A'),
           (N'Electronics', N'Block B'),
           (N'Information Technology', N'Block A');
END;
GO

/* Add sample students/courses only after confirming the expected department IDs.
   These inserts are guarded to avoid duplicate sample records. */
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE Email=N'arun@example.com')
   AND EXISTS (SELECT 1 FROM dbo.Departments WHERE DepartmentId=1)
BEGIN
    INSERT INTO dbo.Students (Name, Email, Age, DepartmentId)
    VALUES (N'Arun Kumar', N'arun@example.com', 21, 1);
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE Email=N'priya@example.com')
   AND EXISTS (SELECT 1 FROM dbo.Departments WHERE DepartmentId=3)
BEGIN
    INSERT INTO dbo.Students (Name, Email, Age, DepartmentId)
    VALUES (N'Priya Devi', N'priya@example.com', 22, 3);
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE Email=N'kavin@example.com')
   AND EXISTS (SELECT 1 FROM dbo.Departments WHERE DepartmentId=1)
BEGIN
    INSERT INTO dbo.Students (Name, Email, Age, DepartmentId)
    VALUES (N'Kavin Raj', N'kavin@example.com', 20, 1);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Courses WHERE CourseName=N'C# Programming')
   AND EXISTS (SELECT 1 FROM dbo.Departments WHERE DepartmentId=1)
BEGIN
    INSERT INTO dbo.Courses (CourseName, DurationMonths, Fee, DepartmentId)
    VALUES (N'C# Programming', 3, 8500, 1);
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Courses WHERE CourseName=N'SQL Server')
   AND EXISTS (SELECT 1 FROM dbo.Departments WHERE DepartmentId=1)
BEGIN
    INSERT INTO dbo.Courses (CourseName, DurationMonths, Fee, DepartmentId)
    VALUES (N'SQL Server', 2, 6000, 1);
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Courses WHERE CourseName=N'Web Development')
   AND EXISTS (SELECT 1 FROM dbo.Departments WHERE DepartmentId=3)
BEGIN
    INSERT INTO dbo.Courses (CourseName, DurationMonths, Fee, DepartmentId)
    VALUES (N'Web Development', 4, 12000, 3);
END;
GO
