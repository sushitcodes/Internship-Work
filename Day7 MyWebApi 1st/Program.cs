using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
//it is used for the giving connection to other computer connected to the local network.
//builder.WebHost.UseUrls(
//    "http://0.0.0.0:5284",
//    "https://0.0.0.0:7239"
//);
// 1. Add DbContext

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Controllers
builder.Services.AddControllers();

// 3. Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Todo API", Version = "v1" });
});

var app = builder.Build();

// 4. Enable Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
    });
}

// 5. Use HTTPS
//app.UseHttpsRedirection();

// 6. Map Controllers
app.MapControllers();

// 7. Create database if not exists
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    dbContext.Database.EnsureCreated();
}

// 8. Run
app.Run();