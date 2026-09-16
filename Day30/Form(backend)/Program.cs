using Form.Exceptions;
using Form.FileStorage;
using Form.Interfaces;
using Form.Persistence;
using Form.Persistence.Repositories;
using Form.Repositories;
using Form.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, config) =>
{
config
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        fileSizeLimitBytes: 50_000_000,     // roll if a single file exceeds 50MB
         rollOnFileSizeLimit: true,          // if exceeded, start a new file mid-day
        shared: false);                    // single process writer

});
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
builder.Services.AddScoped<IClassRoomRepository, ClassRoomRepository>();
builder.Services.AddScoped<IClassRoomService, ClassRoomService>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IGradeService, GradeService>(); builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

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
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Staff", "Admin"));
});// --- CORS: lets the Vite dev server (different port) call this API ---
builder.Services.AddCors(options =>
{
options.AddPolicy("AllowFrontend", policy =>
    policy.WithOrigins("http://localhost:5173")
          .AllowAnyHeader()
          .AllowAnyMethod()
    .AllowCredentials());
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var ex = feature?.Error;
        var traceId = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;

        // Domain exceptions are expected — log at Warning, no stack trace needed.
        // Unexpected exceptions are bugs — log at Error with the exception.
        var (status, message, isExpected) = ex switch
        {
            ValidateException v => (StatusCodes.Status400BadRequest, v.Message, true),
            NotFoundException n => (StatusCodes.Status404NotFound, n.Message, true),
            ConflictException c => (StatusCodes.Status409Conflict, c.Message, true),
            _ => (StatusCodes.Status500InternalServerError,
                                    "An unexpected error occurred. Please try again.",
                                    false)
        };

        if (isExpected)
        {
            Log.Warning("Domain error on {Method} {Path}: {Message} (TraceId={TraceId})",
                context.Request.Method, context.Request.Path, ex!.Message, traceId);
        }
        else
        {
            Log.Error(ex,
                "Unhandled exception on {Method} {Path} (TraceId={TraceId})",
                context.Request.Method, context.Request.Path, traceId);
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            message,
            traceId = isExpected ? null : traceId
        });
    });
});


app.UseStaticFiles();
    app.UseCors("AllowFrontend");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();

