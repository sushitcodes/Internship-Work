namespace StudentCrudApp.Models
{
    // Plain data model. No database logic lives here — ADO.NET code
    // never touches this class directly except to read/write its properties.
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public double Gpa { get; set; }

        public override string ToString()
        {
            return $"[{Id}] {FullName,-20} | {Course,-15} | GPA: {Gpa:F2}";
        }
    }
}

