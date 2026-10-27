using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class StudentService(StudentRepository repository, DepartmentRepository departments, AuditLogRepository audit)
{
    public Task<List<Student>> GetAllAsync() => repository.GetAllAsync();
    public Task<Student?> GetByIdAsync(int id) => repository.GetByIdAsync(id);
    public Task<List<Student>> SearchByNameAsync(string name) => repository.SearchByNameAsync(name);
    public Task<List<Student>> GetByDepartmentAsync(int id) => repository.GetByDepartmentAsync(id);

    public async Task<string> AddAsync(string name, string email, int age, int departmentId)
    {
        string? error = Validate(name, email, age);
        if (error != null) return error;
        var department = await departments.GetByIdAsync(departmentId);
        if (department == null || !department.IsActive) return "Choose an existing active department.";
        int id = await repository.AddAsync(name.Trim(), email.Trim(), age, departmentId);
        await audit.AddAsync("Student", id, "Created", $"Student '{name}' registered");
        return $"Student registered. ID: {id}";
    }

    public async Task<string> UpdateAsync(int id, string name, string email, int age, int departmentId)
    {
        string? error = Validate(name, email, age);
        if (error != null) return error;
        var department = await departments.GetByIdAsync(departmentId);
        if (department == null || !department.IsActive) return "Choose an existing active department.";
        bool updated = await repository.UpdateAsync(id, name.Trim(), email.Trim(), age, departmentId);
        if (!updated) return "Student not found or inactive.";
        await audit.AddAsync("Student", id, "Updated", $"Student '{name}' updated");
        return "Student updated.";
    }

    public async Task<string> DeactivateAsync(int id)
    {
        bool changed = await repository.DeactivateAsync(id);
        if (!changed) return "Student not found or already inactive.";
        await audit.AddAsync("Student", id, "Deactivated", "Student deactivated");
        return "Student deactivated.";
    }

    private static string? Validate(string name, string email, int age)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Student name is required.";
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return "Enter a valid email address.";
        if (age < 17 || age > 60) return "Age must be between 17 and 60.";
        return null;
    }
}
