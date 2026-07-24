using SchoolManagementSystem.Data;
using SchoolManagementSystem.Exceptions;
using SchoolManagementSystem.Models;

namespace SchoolManagementSystem.Services
{
    // EventArgs subclass — the standard .NET pattern for passing event data.
    public class StudentEventArgs : EventArgs
    {
        public Student Student { get; }
        public StudentEventArgs(Student student) => Student = student;
    }

    public class SchoolService
    {
        private readonly JsonDataStore<Student> _studentStore = new("students.json");
        private readonly JsonDataStore<Teacher> _teacherStore = new("teachers.json");

        private List<Student> _students = new();
        private List<Teacher> _teachers = new();

        // DELEGATE + EVENT — other code (like Program.cs) can "subscribe" to
        // this without SchoolService knowing anything about who's listening.
        // EventHandler<T> is a built-in generic delegate type.
        public event EventHandler<StudentEventArgs>? StudentEnrolled;

        // ASYNC/AWAIT — loading is awaited once at startup.
        public async Task InitializeAsync()
        {
            _students = await _studentStore.LoadAsync();
            _teachers = await _teacherStore.LoadAsync();
        }

        public async Task<Student> AddStudentAsync(string name, int age, double gpa)
        {
            int newId = _students.Any() ? _students.Max(s => s.Id) + 1 : 1;
            var student = new Student(newId, name, age, gpa);

            _students.Add(student);
            await _studentStore.SaveAsync(_students);
            return student;
        }

        public async Task<Teacher> AddTeacherAsync(string name, int age, string subject)
        {
            int newId = _teachers.Any() ? _teachers.Max(t => t.Id) + 1 : 1;
            var teacher = new Teacher(newId, name, age, subject);

            _teachers.Add(teacher);
            await _teacherStore.SaveAsync(_teachers);
            return teacher;
        }

        public async Task EnrollStudentAsync(int studentId, string courseName)
        {
            // COLLECTIONS — LINQ over a List<T>
            Student student = _students.FirstOrDefault(s => s.Id == studentId)
                ?? throw new EnrollmentException($"No student with Id {studentId}.");

            student.EnrollInCourse(courseName);
            await _studentStore.SaveAsync(_students);

            // RAISE THE EVENT — anyone subscribed gets notified right here.
            StudentEnrolled?.Invoke(this, new StudentEventArgs(student));
        }

        public List<Student> GetAllStudents() => _students;
        public List<Teacher> GetAllTeachers() => _teachers;

        // LINQ — filtering, ordering, grouping, all via lambda expressions.
        public List<Student> GetHonorRoll() =>
            _students.Where(s => s.Gpa >= 3.5)
                     .OrderByDescending(s => s.Gpa)
                     .ToList();

        public Dictionary<Enums.Grade, List<Student>> GroupByLetterGrade() =>
            _students.GroupBy(s => s.GetLetterGrade())
                     .ToDictionary(g => g.Key, g => g.ToList());

        public double GetAverageGpa() =>
            _students.Any() ? _students.Average(s => s.Gpa) : 0;

        // Returns Person so we can demonstrate POLYMORPHISM at the call site:
        // one list, two different runtime types, each printing its own summary.
        public List<Person> GetEveryone() =>
            _students.Cast<Person>().Concat(_teachers.Cast<Person>()).ToList();
    }
}
