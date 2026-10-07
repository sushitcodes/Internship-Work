using Form.Authorization;
using Form.Exceptions;
using Form.FileStorage;
using Form.Hubs;
using Form.Interface;
using Form.Interfaces;
using Form.Persistence;
using Form.Persistence.Repositories;
using Form.Repositories;
using Form.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

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
            fileSizeLimitBytes: 50_000_000,
            rollOnFileSizeLimit: true,
            shared: false);
});

// --- Fail fast on a missing or weak JWT key (clear message instead of a NullReference) ---
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException(
        "Jwt:Key is missing or shorter than 32 bytes. Set it with: " +
        "dotnet user-secrets set \"Jwt:Key\" \"<a long random string>\"");

// --- Database ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Dependency injection ---
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

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
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IAuthorizationHandler, ClassTeacherAuthorizationHandler>();
builder.Services.AddScoped<IReportCardPdfService, ReportCardPdfService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IBulkImportService, BulkImportService>();

// Background work
builder.Services.AddSingleton<EmailQueue>();
builder.Services.AddHostedService<EmailSenderWorker>();
builder.Services.AddHostedService<TokenCleanupService>();

// --- Behind a reverse proxy the rate limiter must see the real client IP ---
// By default only loopback proxies are trusted, so a random client cannot spoof X-Forwarded-For.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// --- Authentication (JWT read from the httpOnly cookie) ---
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            NameClaimType = "sub",
            ClockSkew = TimeSpan.FromSeconds(30),   // default is 5 minutes
        };
        options.Events = new JwtBearerEvents
        {
            // Cookie only. SignalR also sends cookies on the WebSocket handshake,
            // so the old ?access_token= query string path is not needed.
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("jwt", out var token))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEdit", policy => policy.RequireRole("Staff", "Admin"));
    options.AddPolicy("CanDelete", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Staff", "Admin"));
    options.AddPolicy("ClassTeacherOrAdmin", policy =>
        policy.Requirements.Add(new ClassTeacherRequirement()));
});

// --- CORS (origins come from appsettings so production does not need a code change) ---
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// --- Rate limiting: one bucket PER CLIENT IP, not one bucket for everybody ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("AuthPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 20,
                QueueLimit = 0,
            }));
});

var app = builder.Build();

app.UseForwardedHeaders();

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

        // Client disconnected (e.g. browser navigated away, component unmounted, or the
        // frontend retried after a token refresh and cancelled the original request).
        // This is normal — log at Debug level and return without writing a response body,
        // because the connection is already gone.
        if (ex is OperationCanceledException or TaskCanceledException
            || context.RequestAborted.IsCancellationRequested)
        {
            Log.Debug("Request cancelled by client on {Method} {Path}",
                context.Request.Method, context.Request.Path);
            context.Response.StatusCode = 499; // "Client Closed Request" (nginx convention)
            return;
        }

        var (status, message, isExpected) = ex switch
        {
            ValidateException v => (StatusCodes.Status400BadRequest, v.Message, true),
            NotFoundException n => (StatusCodes.Status404NotFound, n.Message, true),
            ConflictException c => (StatusCodes.Status409Conflict, c.Message, true),
            // Safety net: a race that slips past our checks hits the unique index
            // and becomes a clean 409 instead of "unexpected error".
            DbUpdateException d when d.IsUniqueViolation() =>
                (StatusCodes.Status409Conflict, "That record already exists.", true),
            _ => (StatusCodes.Status500InternalServerError,
                  "An unexpected error occurred. Please try again.", false)
        };

        if (isExpected)
            Log.Warning("Domain error on {Method} {Path}: {Message} (TraceId={TraceId})",
                context.Request.Method, context.Request.Path, ex!.Message, traceId);
        else
            Log.Error(ex, "Unhandled exception on {Method} {Path} (TraceId={TraceId})",
                context.Request.Method, context.Request.Path, traceId);

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            message,
            traceId = isExpected ? null : traceId
        });
    });
});

app.UseSerilogRequestLogging();   // one line per request: method, path, status, ms

app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notifications").RequireAuthorization();
app.MapControllers();

app.Run();