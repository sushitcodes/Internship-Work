namespace SchoolManagementSystem.Interfaces
{
    // INTERFACE — a contract. Any class implementing this PROMISES to provide
    // these members, but the interface itself has no implementation at all
    // (unlike an abstract class, which can share real code between subclasses).
    public interface IEnrollable
    {
        void EnrollInCourse(string courseName);
        List<string> EnrolledCourses { get; }
    }
}
