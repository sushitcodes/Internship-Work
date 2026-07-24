//                                                           1.Constructor
//    //Car myCar = new Car("Toyota");
////myCar.Accelereate();

//                                                          2.Employ Management System( Taxabel, Employee, FullTImeEmployee , Cotractor) 

//class Program
//{
//    static void Main(string[] args)
//    {
//        List<Employe> employees = new List<Employe>();
//        employees.Add(new FullTimeEmployee("Sushit", 50000));
//        employees.Add(new Contractor("Ram", 25, 160));
//        Console.WriteLine("======Employee Salary Report:=======\n");
//        foreach (Employe emp in employees)
//        {
//            emp.ShowDetails();
//            if (emp is Taxable taxableEmp)
//            {
//                Console.WriteLine($"Tax: {taxableEmp.CalculateTax():C}\n");

//            }
//        }
//    }
//}

//                                                                  3.Advanced C# Features
//Collections(Lists, Dictionaries, Queues, Stacks
//class Program
//{
//    static void Main()
//    {
// ---------- LIST ----------
// Ordered, resizable collection — like an array that can grow
//    List<string> fruits = new List<string>();
//    fruits.Add("Apple");
//    fruits.Add("Banana");
//    fruits.Add("Mango");
//    fruits.Remove("Banana");
//    fruits.Insert(1, "Orange");

//Console.WriteLine($"---- List: ----");
//foreach (string fruit in fruits)
//    {
//        Console.WriteLine(fruit);
//    }
//    Console.WriteLine($"Count: {fruits.Count}\n");

//// ---------- DICTIONARY ----------
//// Key-value pairs — fast lookup by unique key
//Dictionary<string, int> ages = new Dictionary<string, int>();
//ages.Add("Sush", 22);
//ages.Add("Ram", 25);
//ages["Shyam"] = 30; // this syntax also adds/updates

//Console.WriteLine("---- Dictionary ----");
//foreach (KeyValuePair<string, int> entry in ages)
//{
//    Console.WriteLine($"{entry.Key} is {entry.Value} years old");
//}

//if (ages.ContainsKey("Sush"))
//{
//    Console.WriteLine($"Found Sush: {ages["Sush"]}\n");
//}

//// ---------- QUEUE ----------
//        //// First-In-First-Out (FIFO) — like a line at a ticket counter
//        Queue<string> ticketLine = new Queue<string>();
//        ticketLine.Enqueue("Customer 1");
//        ticketLine.Enqueue("Customer 2");
//        ticketLine.Enqueue("Customer 3");

//        Console.WriteLine("---- Queue ----");
//        Console.WriteLine($"Next to serve: {ticketLine.Peek()}"); // look without removing
//        Console.WriteLine($"Serving: {ticketLine.Dequeue()}");    // remove and return
//        Console.WriteLine($"Serving: {ticketLine.Dequeue()}");
//        Console.WriteLine($"Remaining in line: {ticketLine.Count}\n");

//        //// ---------- STACK ----------
//        //// Last-In-First-Out (LIFO) — like a stack of plates
//        Stack<string> browserHistory = new Stack<string>();
//        browserHistory.Push("google.com");
//        browserHistory.Push("github.com");
//        browserHistory.Push("stackoverflow.com");

//        Console.WriteLine("---- Stack ----");
//        Console.WriteLine($"Current page: {browserHistory.Peek()}");
//        Console.WriteLine($"Going back to: {browserHistory.Pop()}");
//        Console.WriteLine($"Going back to: {browserHistory.Pop()}");
//        Console.WriteLine($"Going back to: {browserHistory.Pop()}");

//    }
//}

//Generics and LINQ (Language-Integrated Query)

// Program.cs
//using System.Runtime.CompilerServices;

//class Program
//{
//    public static void Main(string[] args)
//    {
//        // ---------- GENERICS ----------
//        GenericBox<int> intBox = new GenericBox<int>();
//        intBox.Store(42);
//        Console.WriteLine($"Int box holds: {intBox.Retrieve()}");

//        GenericBox<string> stringBox = new GenericBox<string>();
//        stringBox.Store("Hello Generics");
//        Console.WriteLine($"String box holds: {stringBox.Retrieve()}\n");

//----------LINQ----------
//    List<Product> products = new List<Product>
//    {
//        new Product("Laptop", 800, "Electronics"),
//        new Product("Mouse", 20, "Electronics"),
//        new Product("Desk", 150, "Furniture"),
//        new Product("Chair", 90, "Furniture"),
//        new Product("Monitor", 200, "Electronics")
//    };

//// Filter: products under $200
//var affordable = products.Where(p => p.Price < 200);
//Console.WriteLine("---- Affordable products (LINQ Where) ----");
//foreach (var p in affordable)
//    Console.WriteLine($"{p.Name} - ${p.Price}");

//// Sort: by price ascending
//var sortedByPrice = products.OrderBy(p => p.Price);
//Console.WriteLine("\n---- Sorted by price (LINQ OrderBy) ----");
//foreach (var p in sortedByPrice)
//    Console.WriteLine($"{p.Name} - ${p.Price}");

//        // Group: by category
//        var grouped = products.GroupBy(p => p.Category);
//        Console.WriteLine("\n---- Grouped by category (LINQ GroupBy) ----");
//        foreach (var group in grouped)
//        {
//            Console.WriteLine($"{group.Key}:");
//            foreach (var p in group)
//                Console.WriteLine($"  {p.Name}");
//        }

//        // Aggregate: total value of all products
//        double totalValue = products.Sum(p => p.Price);
//        Console.WriteLine($"\nTotal inventory value: ${totalValue}");

//    }
//}

//Delegates, Events, and Lambda Expressions
//class Program
//{
//    // A plain method matching the delegate's shape
//    static void LogAlarm(string message)
//    {
//        Console.WriteLine($"[LOG] {message}");
//    }

//    static void Main()
//    {
//        // ---------- DELEGATE (basic use) ----------
//        AlarmHandler handler = LogAlarm;
//        handler("Direct delegate call");

//        // ---------- EVENT ----------
//        AlarmClock clock = new AlarmClock();

//        // Subscribe using a named method
//        clock.OnAlarmRing += LogAlarm;

//        // Subscribe using a LAMBDA EXPRESSION — an inline anonymous method
//        clock.OnAlarmRing += (message) => Console.WriteLine($"[PHONE NOTIFICATION] {message}");

//        clock.RingAlarm(); // triggers BOTH subscribers

//        // ---------- LAMBDA EXPRESSIONS on their own ----------
//        Func<int, int, int> add = (a, b) => a + b; // takes 2 ints, returns int
//        Console.WriteLine($"\nLambda add: {add(5, 3)}");

//        Action<string> greet = name => Console.WriteLine($"Hello, {name}!"); // takes input, returns nothing
//        greet("Sush");

//        Predicate<int> isEven = n => n % 2 == 0; // takes input, returns bool
//        Console.WriteLine($"Is 4 even? {isEven(4)}");

//    }


//Asynchronous Programming (async/await)

//class Program
//{
//    static async Task Main()   // Main can be async too, using Task as return type
//    {
//        WeatherService service = new WeatherService();

//        Console.WriteLine("Requesting weather...");

// await pauses HERE without blocking the whole thread — the program can do other things
//        string result = await service.GetWeatherAsync("Kathmandu");

//        Console.WriteLine(result);
//        Console.WriteLine("Done!");

//        // ---------- Running multiple async tasks together ----------
//        Console.WriteLine("\nFetching multiple cities at once:");
//        Task<string> task1 = service.GetWeatherAsync("Tokyo");
//        Task<string> task2 = service.GetWeatherAsync("London");

//        // Both run concurrently instead of one after another
//        string[] results = await Task.WhenAll(task1, task2);
//        foreach (string r in results)
//            Console.WriteLine(r);

//        Console.ReadKey();
//    }
//}

//File Handling (Reading/Writing files, Streams)
// Program.cs
    class Program
    {
        static void Main()
        {
            string path = "notes.txt";
            NoteManager manager = new NoteManager(path);

            manager.WriteNote("First note: Learning C# file handling.");
            manager.AppendNote("Second note: Streams and file I/O.");
            manager.WriteWithStream("Third note: added via StreamWriter.");

            Console.WriteLine("\n---- File Contents ----");
            Console.WriteLine(manager.ReadNote());

            Console.ReadKey();
        }
    }
