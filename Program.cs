using StudentCourseManagement.Helpers;
using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;
using StudentCourseManagement.Services;

namespace StudentCourseManagement;

internal class Program
{
    private static readonly DepartmentRepository DepartmentRepository = new();
    private static readonly StudentRepository StudentRepository = new();
    private static readonly CourseRepository CourseRepository = new();
    private static readonly EnrollmentRepository EnrollmentRepository = new();
    private static readonly AuditLogRepository AuditLogRepository = new();

    private static readonly DepartmentService DepartmentService = new(DepartmentRepository, AuditLogRepository);
    private static readonly StudentService StudentService = new(StudentRepository, DepartmentRepository, AuditLogRepository);
    private static readonly CourseService CourseService = new(CourseRepository, DepartmentRepository, AuditLogRepository);
    private static readonly EnrollmentService EnrollmentService = new(EnrollmentRepository, StudentRepository, CourseRepository, AuditLogRepository);
    private static readonly AuditLogService AuditLogService = new(AuditLogRepository);

    private static async Task Main()
    {
        Console.Title = "Student Course Management System";
        Console.WriteLine("Student Course Management System");
        Console.WriteLine("--------------------------------");

        while (true)
        {
            Console.WriteLine("\n1. Department Management");
            Console.WriteLine("2. Student Management");
            Console.WriteLine("3. Course Management");
            Console.WriteLine("4. Enrollment Management");
            Console.WriteLine("5. Reports");
            Console.WriteLine("6. View Recent Audit Logs");
            Console.WriteLine("0. Exit");
            Console.Write("Choose an option: ");
            string choice = Console.ReadLine() ?? "";

            try
            {
                switch (choice)
                {
                    case "1": await DepartmentMenu(); break;
                    case "2": await StudentMenu(); break;
                    case "3": await CourseMenu(); break;
                    case "4": await EnrollmentMenu(); break;
                    case "5": await ReportsMenu(); break;
                    case "6": await ShowAuditLogs(); break;
                    case "0": return;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static async Task DepartmentMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Department Management ---");
            Console.WriteLine("1. Add department\n2. List departments\n3. Search department by ID\n4. Update department\n5. Deactivate department\n0. Back");
            Console.Write("Choose: ");
            string choice = Console.ReadLine() ?? "";
            if (choice == "0") return;

            if (choice == "1")
            {
                Console.Write("Department name: ");
                string name = Console.ReadLine() ?? "";
                Console.Write("Location (optional): ");
                string location = Console.ReadLine() ?? "";
                Console.WriteLine(await DepartmentService.AddAsync(name, location));
            }
            else if (choice == "2") PrintDepartments(await DepartmentService.GetAllAsync());
            else if (choice == "3")
            {
                int id = ReadInt("Department ID: ");
                var item = await DepartmentService.GetByIdAsync(id);
                if (item == null) Console.WriteLine("Department not found.");
                else PrintDepartment(item);
            }
            else if (choice == "4")
            {
                int id = ReadInt("Department ID: ");
                Console.Write("New department name: ");
                string name = Console.ReadLine() ?? "";
                Console.Write("New location (optional): ");
                string location = Console.ReadLine() ?? "";
                Console.WriteLine(await DepartmentService.UpdateAsync(id, name, location));
            }
            else if (choice == "5") Console.WriteLine(await DepartmentService.DeactivateAsync(ReadInt("Department ID: ")));
            else Console.WriteLine("Invalid option.");
        }
    }

    private static async Task StudentMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Student Management ---");
            Console.WriteLine("1. Register student\n2. List students\n3. Search by ID\n4. Search by name\n5. Filter by department\n6. Update student\n7. Deactivate student\n0. Back");
            Console.Write("Choose: ");
            string choice = Console.ReadLine() ?? "";
            if (choice == "0") return;

            if (choice == "1")
            {
                Console.Write("Name: "); string name = Console.ReadLine() ?? "";
                Console.Write("Email: "); string email = Console.ReadLine() ?? "";
                int age = ReadInt("Age: ");
                int dept = ReadInt("Department ID: ");
                Console.WriteLine(await StudentService.AddAsync(name, email, age, dept));
            }
            else if (choice == "2") PrintStudents(await StudentService.GetAllAsync());
            else if (choice == "3")
            {
                var item = await StudentService.GetByIdAsync(ReadInt("Student ID: "));
                if (item == null) Console.WriteLine("Student not found."); else PrintStudent(item);
            }
            else if (choice == "4")
            {
                Console.Write("Name contains: ");
                PrintStudents(await StudentService.SearchByNameAsync(Console.ReadLine() ?? ""));
            }
            else if (choice == "5") PrintStudents(await StudentService.GetByDepartmentAsync(ReadInt("Department ID: ")));
            else if (choice == "6")
            {
                int id = ReadInt("Student ID: ");
                Console.Write("Name: "); string name = Console.ReadLine() ?? "";
                Console.Write("Email: "); string email = Console.ReadLine() ?? "";
                int age = ReadInt("Age: ");
                int dept = ReadInt("Department ID: ");
                Console.WriteLine(await StudentService.UpdateAsync(id, name, email, age, dept));
            }
            else if (choice == "7") Console.WriteLine(await StudentService.DeactivateAsync(ReadInt("Student ID: ")));
            else Console.WriteLine("Invalid option.");
        }
    }

    private static async Task CourseMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Course Management ---");
            Console.WriteLine("1. Add course\n2. List courses\n3. Search course by ID\n4. Filter by department\n5. Update course\n6. Deactivate course\n0. Back");
            Console.Write("Choose: ");
            string choice = Console.ReadLine() ?? "";
            if (choice == "0") return;

            if (choice == "1")
            {
                Console.Write("Course name: "); string name = Console.ReadLine() ?? "";
                int duration = ReadInt("Duration in months: ");
                decimal fee = ReadDecimal("Fee: ");
                int dept = ReadInt("Department ID: ");
                Console.WriteLine(await CourseService.AddAsync(name, duration, fee, dept));
            }
            else if (choice == "2") PrintCourses(await CourseService.GetAllAsync());
            else if (choice == "3")
            {
                var item = await CourseService.GetByIdAsync(ReadInt("Course ID: "));
                if (item == null) Console.WriteLine("Course not found."); else PrintCourse(item);
            }
            else if (choice == "4") PrintCourses(await CourseService.GetByDepartmentAsync(ReadInt("Department ID: ")));
            else if (choice == "5")
            {
                int id = ReadInt("Course ID: ");
                Console.Write("Course name: "); string name = Console.ReadLine() ?? "";
                int duration = ReadInt("Duration in months: ");
                decimal fee = ReadDecimal("Fee: ");
                int dept = ReadInt("Department ID: ");
                Console.WriteLine(await CourseService.UpdateAsync(id, name, duration, fee, dept));
            }
            else if (choice == "6") Console.WriteLine(await CourseService.DeactivateAsync(ReadInt("Course ID: ")));
            else Console.WriteLine("Invalid option.");
        }
    }

    private static async Task EnrollmentMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Enrollment Management ---");
            Console.WriteLine("1. Enroll student\n2. List enrollments\n3. View courses for a student\n4. View students for a course\n5. Complete enrollment\n6. Cancel enrollment\n0. Back");
            Console.Write("Choose: ");
            string choice = Console.ReadLine() ?? "";
            if (choice == "0") return;

            if (choice == "1")
                Console.WriteLine(await EnrollmentService.EnrollAsync(ReadInt("Student ID: "), ReadInt("Course ID: ")));
            else if (choice == "2") PrintEnrollments(await EnrollmentService.GetAllAsync());
            else if (choice == "3") PrintEnrollments(await EnrollmentService.GetForStudentAsync(ReadInt("Student ID: ")));
            else if (choice == "4") PrintEnrollments(await EnrollmentService.GetForCourseAsync(ReadInt("Course ID: ")));
            else if (choice == "5") Console.WriteLine(await EnrollmentService.ChangeStatusAsync(ReadInt("Enrollment ID: "), "Completed"));
            else if (choice == "6") Console.WriteLine(await EnrollmentService.ChangeStatusAsync(ReadInt("Enrollment ID: "), "Cancelled"));
            else Console.WriteLine("Invalid option.");
        }
    }

    private static async Task ReportsMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Reports ---");
            Console.WriteLine("1. Student count by department\n2. Enrollment count by course\n3. Students without enrollments\n4. Courses without enrollments\n0. Back");
            Console.Write("Choose: ");
            string choice = Console.ReadLine() ?? "";
            if (choice == "0") return;
            if (choice == "1")
                foreach (var row in await StudentRepository.GetStudentCountByDepartmentAsync()) Console.WriteLine($"{row.Name}: {row.Count}");
            else if (choice == "2")
                foreach (var row in await CourseRepository.GetEnrollmentCountAsync()) Console.WriteLine($"{row.Name}: {row.Count}");
            else if (choice == "3")
                PrintStudents(await StudentRepository.GetWithoutEnrollmentsAsync());
            else if (choice == "4")
                PrintCourses(await CourseRepository.GetWithoutEnrollmentsAsync());
            else Console.WriteLine("Invalid option.");
        }
    }

    private static async Task ShowAuditLogs()
    {
        foreach (var log in await AuditLogService.GetRecentAsync(10))
            Console.WriteLine($"{log.CreatedAt:g} | {log.EntityName} #{log.EntityId} | {log.Action} | {log.Details}");
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int value)) return value;
            Console.WriteLine("Enter a valid whole number.");
        }
    }

    private static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine(), out decimal value)) return value;
            Console.WriteLine("Enter a valid amount.");
        }
    }

    private static void PrintDepartments(IEnumerable<Department> items) { foreach (var x in items) PrintDepartment(x); }
    private static void PrintDepartment(Department x) => Console.WriteLine($"{x.DepartmentId} | {x.DepartmentName} | {x.Location} | Active: {x.IsActive}");
    private static void PrintStudents(IEnumerable<Student> items) { foreach (var x in items) PrintStudent(x); }
    private static void PrintStudent(Student x) => Console.WriteLine($"{x.StudentId} | {x.Name} | {x.Email} | Age {x.Age} | Dept: {x.DepartmentName} ({x.DepartmentId}) | Active: {x.IsActive}");
    private static void PrintCourses(IEnumerable<Course> items) { foreach (var x in items) PrintCourse(x); }
    private static void PrintCourse(Course x) => Console.WriteLine($"{x.CourseId} | {x.CourseName} | {x.DurationMonths} months | {x.Fee:C} | Dept: {x.DepartmentName} ({x.DepartmentId}) | Active: {x.IsActive}");
    private static void PrintEnrollments(IEnumerable<Enrollment> items) { foreach (var x in items) Console.WriteLine($"{x.EnrollmentId} | {x.StudentName} -> {x.CourseName} | {x.EnrolledAt:g} | {x.Status}"); }
}
