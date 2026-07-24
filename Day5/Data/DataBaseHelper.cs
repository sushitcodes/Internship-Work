using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace StudentCrudApp.Data
{
    // Single responsibility: know how to open a connection, and make sure
    // the table we need exists. Nothing about Students' business logic lives here.
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            IConfiguration config = new ConfigurationBuilder()
.SetBasePath(@"G:\basic C#\Day").AddJsonFile("appsettings.json")
                .Build();

            _connectionString = config.GetConnectionString("SchoolDb")
                ?? throw new InvalidOperationException("Connection string 'SchoolDb' not found in appsettings.json");
        }

        // Every CRUD method will call this to get a fresh, open connection.
        // Callers are responsible for wrapping it in a 'using' block so it
        // gets closed/disposed even if an exception happens.
        public SqlConnection GetConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        // Creates the Students table if it doesn't already exist.
        // This mirrors what EnsureCreated() does in EF Core, but we're
        // writing the raw SQL ourselves since this is ADO.NET.
        public void EnsureTableExists()
        {
            const string sql = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Students' AND xtype='U')
                BEGIN
                    CREATE TABLE Students (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        FullName NVARCHAR(100) NOT NULL,
                        Course NVARCHAR(100) NOT NULL,
                        Gpa FLOAT NOT NULL
                    )
                END";

            using SqlConnection connection = GetConnection();
            using SqlCommand command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }
    }
}
