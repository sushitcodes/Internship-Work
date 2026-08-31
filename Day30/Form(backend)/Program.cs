using Form.FileStorage;
using Form.Interface;
using Form.Interfaces;
using Form.Persistence;
using Form.Repositories;
using Form.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// --- Database (SQL Server via EF Core) ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Dependency Injection wiring: this block IS Clean Architecture at runtime.
// Controllers ask for ISubmissionService; .NET hands them a SubmissionService.
// Swap the right-hand side and nothing else in the app needs to change.
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IEmailService, GmailSmtpEmailService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("jwt", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEdit", policy =>
        policy.RequireRole("Staff", "Admin"));

    options.AddPolicy("CanDelete", policy =>
        policy.RequireRole("Admin"));
});// --- CORS: lets the Vite dev server (different port) call this API ---
builder.Services.AddCors(options =>
{
options.AddPolicy("AllowFrontend", policy =>
    policy.WithOrigins("http://localhost:5173")
          .AllowAnyHeader()
          .AllowAnyMethod()
    .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
