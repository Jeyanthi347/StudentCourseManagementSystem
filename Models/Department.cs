namespace StudentCourseManagement.Models;

public class Department
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "";
    public string? Location { get; set; }
    public bool IsActive { get; set; }

    public Department() { }

    public Department(int departmentId, string departmentName, string? location, bool isActive)
    {
        DepartmentId = departmentId;
        DepartmentName = departmentName;
        Location = location;
        IsActive = isActive;
    }
}
