namespace StudentCourseManagement.Models;

public class Course
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = "";
    public int DurationMonths { get; set; }
    public decimal Fee { get; set; }
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; }
    public string DepartmentName { get; set; } = "";

    public Course() { }

    public Course(int courseId, string courseName, int durationMonths, decimal fee,
        int departmentId, bool isActive)
    {
        CourseId = courseId;
        CourseName = courseName;
        DurationMonths = durationMonths;
        Fee = fee;
        DepartmentId = departmentId;
        IsActive = isActive;
    }
}
