using StudentCourseManagement.Models;
using StudentCourseManagement.Repositories;

namespace StudentCourseManagement.Services;

public class AuditLogService(AuditLogRepository repository)
{
    public Task<List<AuditLog>> GetRecentAsync(int count) => repository.GetRecentAsync(count);
}
