using StudentCrudApp.Data;
using StudentCrudApp.Models;

var dbHelper = new DatabaseHelper();
dbHelper.EnsureTableExists();

var repository = new StudentRepository(dbHelper);

bool running = true;

while (running)
{
    PrintMenu();
    string? choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                CreateStudent();
                break;
            case "2":
                ReadAllStudents();
                break;
            case "3":
                ReadOneStudent();
                break;
            case "4":
                UpdateStudent();
                break;
            case "5":
                DeleteStudent();
                break;
            case "0":
                running = false;
                break;
            default:
                Console.WriteLine("Invalid choice, try again.");
                break;
        }
    }
    catch (Exception ex)
    {
        // Catching here means one bad operation doesn't crash the whole app.
        Console.WriteLine($"Something went wrong: {ex.Message}");
    }

    Console.WriteLine();
}

void PrintMenu()
{
    Console.WriteLine("=== Student CRUD (ADO.NET) ===");
    Console.WriteLine("1. Create student");
    Console.WriteLine("2. View all students");
    Console.WriteLine("3. View one student by Id");
    Console.WriteLine("4. Update student");
    Console.WriteLine("5. Delete student");
    Console.WriteLine("0. Exit");
    Console.Write("Choose an option: ");
}

void CreateStudent()
{
    Console.Write("Full name: ");
    string name = Console.ReadLine() ?? "";

    Console.Write("Course: ");
    string course = Console.ReadLine() ?? "";

    Console.Write("GPA: ");
    double gpa = double.Parse(Console.ReadLine() ?? "0");

    var student = new Student { FullName = name, Course = course, Gpa = gpa };
    int newId = repository.Add(student);

    Console.WriteLine($"Created student with Id {newId}.");
}

void ReadAllStudents()
{
    List<Student> students = repository.GetAll();

    if (students.Count == 0)
    {
        Console.WriteLine("No students found.");
        return;
    }

    foreach (Student s in students)
    {
        Console.WriteLine(s);
    }
}

void ReadOneStudent()
{
    Console.Write("Enter Id: ");
    int id = int.Parse(Console.ReadLine() ?? "0");

    Student? student = repository.GetById(id);

    Console.WriteLine(student is null ? "No student found with that Id." : student.ToString());
}

void UpdateStudent()
{
    Console.Write("Enter Id to update: ");
    int id = int.Parse(Console.ReadLine() ?? "0");

    Student? existing = repository.GetById(id);
    if (existing is null)
    {
        Console.WriteLine("No student found with that Id.");
        return;
    }

    Console.Write($"New full name (leave blank to keep '{existing.FullName}'): ");
    string name = Console.ReadLine() ?? "";

    Console.Write($"New course (leave blank to keep '{existing.Course}'): ");
    string course = Console.ReadLine() ?? "";

    Console.Write($"New GPA (leave blank to keep {existing.Gpa}): ");
    string gpaInput = Console.ReadLine() ?? "";

    existing.FullName = string.IsNullOrWhiteSpace(name) ? existing.FullName : name;
    existing.Course = string.IsNullOrWhiteSpace(course) ? existing.Course : course;
    existing.Gpa = string.IsNullOrWhiteSpace(gpaInput) ? existing.Gpa : double.Parse(gpaInput);

    bool updated = repository.Update(existing);
    Console.WriteLine(updated ? "Student updated." : "Update failed.");
}

void DeleteStudent()
{
    Console.Write("Enter Id to delete: ");
    int id = int.Parse(Console.ReadLine() ?? "0");

    bool deleted = repository.Delete(id);
    Console.WriteLine(deleted ? "Student deleted." : "No student found with that Id.");
}
