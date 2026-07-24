class FullTimeEmployee : Employe, Taxable
{
    public FullTimeEmployee(string name, double baseSalary) : base(name, baseSalary) { }

    public override double CalculateSalary()
    {
        return BaseSalary; // full-time employees just get their fixed base salary
    }

    public double CalculateTax()
    {
        return CalculateSalary() * 0.15; // flat 15% tax for example purposes
    }
}