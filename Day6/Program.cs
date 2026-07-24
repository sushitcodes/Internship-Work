using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var host = CreateHostBuilder(args).Build();
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        Console.WriteLine("✅Database created/verified!");
        RunDemo(context);
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
    static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureAppConfiguration((hostingContext, config) =>
          {
              // Force it to load appsettings.json from the execution directory
              config.SetBasePath(Directory.GetCurrentDirectory());
              config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
          })
          .ConfigureServices((context, services) =>
          {
              string connectionString = context.Configuration.GetConnectionString("DefaultConnection");

              // Defensive check: print clear error if string is still missing
              if (string.IsNullOrEmpty(connectionString))
              {
                  throw new InvalidOperationException("Could not find 'DefaultConnection' in appsettings.json. Make sure the file is set to 'Copy if newer'.");
              }

              services.AddDbContext<AppDbContext>(options =>
                  options.UseSqlServer(connectionString));
          });
    static void RunDemo(AppDbContext context)
    {
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("   My EF NET DEMO Starts here hai!");
        Console.WriteLine("═══════════════════════════════════════");
        GetAllUsers(context);
        GetUserById(context, 1);
        GetUserById(context, 100);
        //AddUser(context);
        UpdateUser(context, 2);
        DeleteUser(context, 6);
        DeleteUser(context,7);
        LinqQueries(context);

        SearchUsersByName(context, "Sushit");
        Console.WriteLine();

        GetUsersByAgeRange(context, 25, 35);
        Console.WriteLine();

        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("   My EF NET DEMO COMPLETE!");
        Console.WriteLine("═══════════════════════════════════════");

    } // Calls the method below}
    //READ: Get All Usere

    public static void GetAllUsers(AppDbContext context)
    {
        Console.WriteLine("📋 ALL USERS:");
        var users = context.Users.ToList();

        foreach (var user in users)
            Console.WriteLine($"ID: {user.Id}, Name: {user.Name}, Email: {user.Email}, Age: {user.Age}");
    }
    //READ: Get  Users BY ID

    public static void GetUserById(AppDbContext context, int id)
    {
        Console.WriteLine($"🔍 GET USER BY ID ({id}):");
        var user = context.Users.Find(id);
        if (user != null)
            Console.WriteLine($"Found: {user.Name} ({user.Email})");
        else
            Console.WriteLine($"User with ID {id} not found");
    }
    //CREATE new USER
    public static void AddUser(AppDbContext context)
    {
        Console.WriteLine("➕ ADDING NEW USER:");

        var newUser = new User
        {
            Name = "Prithivi Narayan Shah",
            Email = "prithivi1@email.com",
            Age = 89

        };
        context.Users.Add(newUser);
        context.SaveChanges();

        Console.WriteLine($"✅ User added! ID: {newUser.Id}");

    }

    //Update id=2 Roshan Dahal or Roooshan Dahal
    public static void UpdateUser(AppDbContext context, int id)
    {
        Console.WriteLine($"✏️ UPDATING USER ({id}):");

        // Find the user
        var user = context.Users.Find(id);

        if (user != null)
        {
            // Update properties
            user.Name = "Roooshan Dahal Updated";
            user.Age = 26;

            // Mark as modified (EF tracks changes automatically!)
            context.Entry(user).State = EntityState.Modified;

            // Save changes (generates UPDATE SQL)
            context.SaveChanges();

            Console.WriteLine($"✅ User {id} updated!");
        }
        else
        {
            Console.WriteLine($"❌ User {id} not found");
        }
    }

    //DELETE: Remove User previously I got 4 or 7 add on same name I want to remove one with the id now i try to delete 6 and 7 id 
    public static void DeleteUser(AppDbContext context, int id)
    {
        Console.WriteLine($"🗑️ DELETING USER ({id}):");

        // Find the user
        var user = context.Users.Find(id);

        if (user != null)
        {
            // Remove from context
            context.Users.Remove(user);

            // Save changes (generates DELETE SQL)
            context.SaveChanges();

            Console.WriteLine($"✅ User {id} deleted!");
        }
        else
        {
            Console.WriteLine($"❌ User {id} not found");
        }
    }
    // Use LINQ for the Searching
  public  static void LinqQueries(AppDbContext context)
    {
        Console.WriteLine("\n🔍 LINQ QUERIES:");
        var adults = context.Users.Where(u => u.Age >= 18).Count();
        Console.WriteLine($"Adults (18+): {adults}");

        var youngest = context.Users.Min(u =>u.Age);
        Console.WriteLine($"Youngest age: {youngest}");

        var oldest = context.Users.Max(u => u.Age);
        Console.WriteLine($"Oldest age: {oldest}");

        var sorted = context.Users.OrderBy(u => u.Name).Select(u => u.Name).ToList();
        Console.WriteLine($"Sorted by name: {string.Join(", ", sorted)}");
    }

    //We get search user using name 

    public static void SearchUsersByName(AppDbContext context, string searchTerm)
    {
        Console.WriteLine($"🔍 SEARCHING FOR: '{searchTerm}'");
        var results = context.Users
            .Where(u => u.Name.Contains(searchTerm))
            .ToList();

        if (results.Any())
        {
            foreach (var user in results)
                Console.WriteLine($"Found: {user.Name} ({user.Email})");
        }
        else
        {
            Console.WriteLine($"No users found with '{searchTerm}'");
        }
    }

    //We get the search user using the Age range
    public static void GetUsersByAgeRange(AppDbContext context, int minAge, int maxAge)
    {
        Console.WriteLine($"📋 USERS BETWEEN {minAge} AND {maxAge}:");
        var users = context.Users
            .Where(u => u.Age >= minAge && u.Age <= maxAge)
            .OrderBy(u => u.Age)
            .ToList();

        if (users.Any())
        {
            foreach (var user in users)
                Console.WriteLine($"Age {user.Age}: {user.Name}");
        }
        else
        {
            Console.WriteLine($"No users found in age range {minAge}-{maxAge}");
        }
    }

}