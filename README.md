# Student Course Management System

A beginner-friendly C# console application using .NET 10, ADO.NET (`Microsoft.Data.SqlClient`) and SQL Server Express. The project is organized into Models, Repositories, Services and Helpers.

## Features

- Department: add, list, search by ID, update, deactivate
- Student: register, list, search by ID/name, filter by department, update, deactivate
- Course: add, list, search by ID, filter by department, update, deactivate
- Enrollment: enroll a student, list enrollments, view enrollments by student/course, complete or cancel an enrollment
- Reports: student count by department, enrollment count by course, students without enrollments, courses without enrollments
- Audit logs for important actions; enrollment changes and their audit records use a SQL transaction
- Parameterized SQL and asynchronous database calls

## Requirements

- Visual Studio 2022 with a .NET 10 SDK, or a compatible .NET 10 SDK
- SQL Server Express with instance name `SQLEXPRESS`
- SQL Server Management Studio (SSMS)

## Important database note

The connection string expects:

```text
Server=.\SQLEXPRESS;Database=StudentCourseDB;Trusted_Connection=True;TrustServerCertificate=True;
```

If you already have an earlier `StudentCourseDB` from another version of the StudentSystem project, its tables may not have the columns this extended version needs. The SQL setup script deliberately does not recreate existing tables, but that means it cannot automatically migrate old tables. **Back up your database and inspect its schema before running `SQL/SetupDatabase.sql`.** If your old Students/Courses/Enrollments tables exist, use a fresh database or add a reviewed migration before running this application.

## Setup

1. Extract the ZIP file.
2. Open `StudentCourseManagement.csproj` in Visual Studio.
3. Open SSMS and connect to `.\SQLEXPRESS`.
4. Review `SQL/SetupDatabase.sql`. Run it only if the existing database schema is compatible or you are using a fresh `StudentCourseDB`.
5. In Visual Studio, restore NuGet packages.
6. Build the project.
7. Run with **Ctrl+F5**.

If SQL Server uses another instance name, update the connection string in `Helpers/DatabaseHelper.cs`.

## Sample department IDs

The setup script inserts sample departments only when `Departments` is empty. It assumes IDs 1 and 3 refer to Computer Science and Information Technology for its guarded sample student/course inserts. If your database already has departments with different IDs, review the sample inserts and choose the correct IDs.

## Folder structure

```text
StudentCourseManagement/
├── Program.cs
├── StudentCourseManagement.csproj
├── Helpers/
│   └── DatabaseHelper.cs
├── Models/
│   ├── Department.cs
│   ├── Student.cs
│   ├── Course.cs
│   ├── Enrollment.cs
│   └── AuditLog.cs
├── Repositories/
│   ├── DepartmentRepository.cs
│   ├── StudentRepository.cs
│   ├── CourseRepository.cs
│   ├── EnrollmentRepository.cs
│   └── AuditLogRepository.cs
├── Services/
│   ├── DepartmentService.cs
│   ├── StudentService.cs
│   ├── CourseService.cs
│   ├── EnrollmentService.cs
│   └── AuditLogService.cs
└── SQL/
    └── SetupDatabase.sql
```

## Notes

- Unique constraints prevent duplicate department names, student emails, course names and student/course enrollment pairs.
- Deactivation is a soft delete: the row remains in SQL Server and `IsActive` becomes `0`.
- Some SQL exceptions, such as duplicate email/course name or duplicate enrollment, are shown by the top-level error handler. The data remains protected by SQL constraints.
- This is a learning project scaffold; test it against a disposable development database before using it with important data.
