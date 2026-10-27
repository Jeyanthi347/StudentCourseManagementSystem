namespace StudentCourseManagement.Models;

public class Student
{
    public int StudentId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public int Age { get; set; }
    public int DepartmentId { get; set; }
    public DateTime AdmissionDate { get; set; }
    public bool IsActive { get; set; }
    public string DepartmentName { get; set; } = "";

    public Student() { }

    public Student(int studentId, string name, string email, int age, int departmentId,
        DateTime admissionDate, bool isActive)
    {
        StudentId = studentId;
        Name = name;
        Email = email;
        Age = age;
        DepartmentId = departmentId;
        AdmissionDate = admissionDate;
        IsActive = isActive;
    }
}
