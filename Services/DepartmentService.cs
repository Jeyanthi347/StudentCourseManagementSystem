using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class DepartmentService(DepartmentRepository repository, AuditLogRepository audit)
{
    public Task<List<Department>> GetAllAsync() => repository.GetAllAsync();
    public Task<Department?> GetByIdAsync(int id) => repository.GetByIdAsync(id);

    public async Task<string> AddAsync(string name, string? location)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Department name is required.";
        int id = await repository.AddAsync(name.Trim(), location?.Trim());
        await audit.AddAsync("Department", id, "Created", $"Department '{name}' created");
        return $"Department added. ID: {id}";
    }

    public async Task<string> UpdateAsync(int id, string name, string? location)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Department name is required.";
        bool updated = await repository.UpdateAsync(id, name.Trim(), location?.Trim());
        if (!updated) return "Department not found or inactive.";
        await audit.AddAsync("Department", id, "Updated", $"Department '{name}' updated");
        return "Department updated.";
    }

    public async Task<string> DeactivateAsync(int id)
    {
        bool changed = await repository.DeactivateAsync(id);
        if (!changed) return "Department not found or already inactive.";
        await audit.AddAsync("Department", id, "Deactivated", "Department deactivated");
        return "Department deactivated.";
    }
}
