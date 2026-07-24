class Contractor : Employe, Taxable
{
    private double hourlyRate;
    private int hoursWorked;
    public Contractor(string name, double hourlyRate, int hoursWorked) : base(name, 0)
    {
        this.hourlyRate = hourlyRate;
        this.hoursWorked = hoursWorked;
    }
    public override double CalculateSalary()
    {
        return hourlyRate * hoursWorked;
    }
    public double CalculateTax()
    {
        return CalculateSalary() * 0.10; // Assuming a flat tax rate of 15%
    }
}