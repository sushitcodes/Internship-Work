using SchoolManagementSystem.Models;
using SchoolManagementSystem.Services;

class Program {
    static async Task Main(string[] args)
    {
        var school = new SchoolService();
await school.InitializeAsync();

// LAMBDA EXPRESSION subscribed to an EVENT — no separate named method needed.
school.StudentEnrolled += (sender, e) =>
    Console.WriteLine($"[event] {e.Student.Name} was just enrolled in a course.");

bool running = true;

while (running)
{
    PrintMenu();
    string? choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1": await AddStudent(); break;
            case "2": await AddTeacher(); break;
            case "3": await Enroll(); break;
            case "4": ShowEveryone(); break;
            case "5": ShowHonorRoll(); break;
            case "6": ShowGroupedByGrade(); break;
            case "0": running = false; break;
            default: Console.WriteLine("Invalid choice."); break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    Console.WriteLine();
}

void PrintMenu()
{
    Console.WriteLine("=== School Management System ===");
    Console.WriteLine("1. Add student");
    Console.WriteLine("2. Add teacher");
    Console.WriteLine("3. Enroll student in a course");
    Console.WriteLine("4. View everyone (polymorphism demo)");
    Console.WriteLine("5. View honor roll (GPA >= 3.5)");
    Console.WriteLine("6. View students grouped by letter grade");
    Console.WriteLine("0. Exit");
    Console.Write("Choose an option: ");
}

async Task AddStudent()
{
    Console.Write("Name: ");
    string name = Console.ReadLine() ?? "";
    Console.Write("Age: ");
    int age = int.Parse(Console.ReadLine() ?? "0");
    Console.Write("GPA: ");
    double gpa = double.Parse(Console.ReadLine() ?? "0");

    Student student = await school.AddStudentAsync(name, age, gpa);
    Console.WriteLine($"Added: {student.GetSummary()}");
}

async Task AddTeacher()
{
    Console.Write("Name: ");
    string name = Console.ReadLine() ?? "";
    Console.Write("Age: ");
    int age = int.Parse(Console.ReadLine() ?? "0");
    Console.Write("Subject: ");
    string subject = Console.ReadLine() ?? "";

    Teacher teacher = await school.AddTeacherAsync(name, age, subject);
    Console.WriteLine($"Added: {teacher.GetSummary()}");
}

async Task Enroll()
{
    Console.Write("Student Id: ");
    int id = int.Parse(Console.ReadLine() ?? "0");
    Console.Write("Course name: ");
    string course = Console.ReadLine() ?? "";

    await school.EnrollStudentAsync(id, course);
}

void ShowEveryone()
{
    // POLYMORPHISM in action: same loop, same method call (GetSummary()),
    // different output depending on whether it's actually a Student or Teacher.
    foreach (Person person in school.GetEveryone())
    {
        Console.WriteLine(person.GetSummary());
    }
}

void ShowHonorRoll()
{
    List<Student> honorRoll = school.GetHonorRoll();
    if (honorRoll.Count == 0) { Console.WriteLine("No students on the honor roll yet."); return; }

    foreach (Student s in honorRoll)
    {
        Console.WriteLine(s);
    }
}

void ShowGroupedByGrade()
{
    var grouped = school.GroupByLetterGrade();
    foreach (var kvp in grouped.OrderBy(g => g.Key))
    {
        Console.WriteLine($"{kvp.Key}: {string.Join(", ", kvp.Value.Select(s => s.Name))}");
    }
}
}
}