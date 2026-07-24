using SchoolManagementSystem.Enums;
using SchoolManagementSystem.Interfaces;

namespace SchoolManagementSystem.Models
{
    // INHERITANCE — Student "is a" Person.
    // Also IMPLEMENTS an interface, so it can be both at once.
    public class Student : Person, IEnrollable, IComparable<Student>
    {
        public double Gpa { get; set; }
        public List<string> EnrolledCourses { get; } = new();

        public Student(int id, string name, int age, double gpa)
            : base(id, name, age) // calls Person's constructor
        {
            Gpa = gpa;
        }

        // POLYMORPHISM — overrides Person's abstract method with Student-specific behavior.
        public override string GetSummary() => $"Student: {Name}, Age {Age}, GPA {Gpa:F2}";

        // Overriding the virtual method too — different greeting than the base default.
        public override string GetGreeting() => $"Hi, I'm {Name}, a student.";

        public void EnrollInCourse(string courseName)
        {
            if (!EnrolledCourses.Contains(courseName))
                EnrolledCourses.Add(courseName);
        }

        public Grade GetLetterGrade()
        {
            // SWITCH EXPRESSION (C# 8+) — compact alternative to switch-case
            return Gpa switch
            {
                >= 3.7 => Grade.A,
                >= 3.0 => Grade.B,
                >= 2.0 => Grade.C,
                >= 1.0 => Grade.D,
                _ => Grade.F
            };
        }

        // OPERATOR OVERLOADING — lets you write `student1 > student2` directly.
        public static bool operator >(Student a, Student b) => a.Gpa > b.Gpa;
        public static bool operator <(Student a, Student b) => a.Gpa < b.Gpa;

        // IComparable<T> — lets List<Student>.Sort() work without a custom comparer.
        public int CompareTo(Student? other) => other is null ? 1 : Gpa.CompareTo(other.Gpa);

        public override string ToString() => GetSummary();
    }
}
