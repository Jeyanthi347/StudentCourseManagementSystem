using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class CourseService(CourseRepository repository, DepartmentRepository departments, AuditLogRepository audit)
{
    public Task<List<Course>> GetAllAsync() => repository.GetAllAsync();
    public Task<Course?> GetByIdAsync(int id) => repository.GetByIdAsync(id);
    public Task<List<Course>> GetByDepartmentAsync(int id) => repository.GetByDepartmentAsync(id);

    public async Task<string> AddAsync(string name, int duration, decimal fee, int departmentId)
    {
        string? error = Validate(name, duration, fee);
        if (error != null) return error;
        var department = await departments.GetByIdAsync(departmentId);
        if (department == null || !department.IsActive) return "Choose an existing active department.";
        int id = await repository.AddAsync(name.Trim(), duration, fee, departmentId);
        await audit.AddAsync("Course", id, "Created", $"Course '{name}' created");
        return $"Course added. ID: {id}";
    }

    public async Task<string> UpdateAsync(int id, string name, int duration, decimal fee, int departmentId)
    {
        string? error = Validate(name, duration, fee);
        if (error != null) return error;
        var department = await departments.GetByIdAsync(departmentId);
        if (department == null || !department.IsActive) return "Choose an existing active department.";
        bool updated = await repository.UpdateAsync(id, name.Trim(), duration, fee, departmentId);
        if (!updated) return "Course not found or inactive.";
        await audit.AddAsync("Course", id, "Updated", $"Course '{name}' updated");
        return "Course updated.";
    }

    public async Task<string> DeactivateAsync(int id)
    {
        bool changed = await repository.DeactivateAsync(id);
        if (!changed) return "Course not found or already inactive.";
        await audit.AddAsync("Course", id, "Deactivated", "Course deactivated");
        return "Course deactivated.";
    }

    private static string? Validate(string name, int duration, decimal fee)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Course name is required.";
        if (duration <= 0) return "Duration must be greater than zero.";
        if (fee < 0) return "Fee cannot be negative.";
        return null;
    }
}
