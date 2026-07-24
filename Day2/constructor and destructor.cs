class Car
{
    public string brand;
    public int speed;
    public Car(string Brand)
    {
        brand = Brand;
        speed = 0;
        Console.WriteLine($"{brand} car created!");

    }
    public void Accelereate()
    {
        speed += 10;
        Console.WriteLine($"{brand} is now going {speed} km/h");

    }
    ~Car()
    {
        Console.WriteLine($"{brand} car destroyed!");
    }
}