abstract class Employe
{
    public string Name;
    protected double BaseSalary;

    public Employe(string name, double baseSalary)
    {
        Name = name;
        BaseSalary = baseSalary;
    }

    public abstract double CalculateSalary();
     public void ShowDetails()
    {
        Console.WriteLine($"Name: {Name},Salary: {CalculateSalary():C}");
    }

}