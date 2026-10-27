namespace StudentCourseManagement.Models;

public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public string Status { get; set; } = "Enrolled";
    public string StudentName { get; set; } = "";
    public string CourseName { get; set; } = "";

    public Enrollment() { }

    public Enrollment(int enrollmentId, int studentId, int courseId, DateTime enrolledAt, string status)
    {
        EnrollmentId = enrollmentId;
        StudentId = studentId;
        CourseId = courseId;
        EnrolledAt = enrolledAt;
        Status = status;
    }
}
