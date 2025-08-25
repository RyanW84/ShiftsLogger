using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using Scalar.AspNetCore;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Always load user secrets if available (use entry assembly fallback)
builder.Configuration.AddUserSecrets(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly(), optional: true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Prevents circular dependency issues
builder
    .Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
    );

// Configure DbContext provider: LocalDB on Windows, LinuxSqlServer on non-Windows
var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
if (isWindows)
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection")
               ?? "Server=(localdb)\\MSSQLLocalDB;Database=ShiftsLoggerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
    builder.Services.AddDbContext<ShiftsLoggerDbContext>(opt => opt.UseSqlServer(conn));
}
else
{
    // prefer LinuxSqlServer, fall back to DefaultConnection
    var linuxConn = builder.Configuration.GetConnectionString("LinuxSqlServer")
                   ?? builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(linuxConn))
        throw new InvalidOperationException("No LinuxSqlServer or DefaultConnection connection string configured in configuration/user secrets.");

    // If connection string contains placeholders we require secrets; otherwise allow embedded creds
    var needsSecrets = linuxConn.Contains("{DB_USER}") || linuxConn.Contains("{DB_PASSWORD}");

    var dbUser = builder.Configuration["Db:User"];
    var dbPass = builder.Configuration["Db:Password"];

    if (needsSecrets)
    {
        if (string.IsNullOrWhiteSpace(dbUser) || string.IsNullOrWhiteSpace(dbPass))
            throw new InvalidOperationException("Db:User and Db:Password must be set in user secrets.");
        linuxConn = linuxConn.Replace("{DB_USER}", dbUser).Replace("{DB_PASSWORD}", dbPass);
    }
    else
    {
        // If secrets exist, allow them to override any embedded User Id / Password
        if (!string.IsNullOrWhiteSpace(dbUser))
            linuxConn = Regex.Replace(linuxConn, @"(User\s*Id\s*=\s*)([^;]+)", $"$1{dbUser}", RegexOptions.IgnoreCase);
        if (!string.IsNullOrWhiteSpace(dbPass))
            linuxConn = Regex.Replace(linuxConn, @"(Password\s*=\s*)([^;]+)", $"$1{dbPass}", RegexOptions.IgnoreCase);
    }

    // Ensure Server uses tcp: prefix
    linuxConn = Regex.Replace(linuxConn, @"Server=(?!tcp:)([^;]+)", "Server=tcp:$1", RegexOptions.IgnoreCase);

    // Ensure Encrypt and TrustServerCertificate are present for local debugging (idempotent)
    if (!Regex.IsMatch(linuxConn, @"\bTrustServerCertificate\s*=", RegexOptions.IgnoreCase))
        linuxConn = linuxConn.TrimEnd(';') + ";TrustServerCertificate=True;";
    if (!Regex.IsMatch(linuxConn, @"\bEncrypt\s*=", RegexOptions.IgnoreCase))
        linuxConn = linuxConn.TrimEnd(';') + ";Encrypt=True;";

    builder.Services.AddDbContext<ShiftsLoggerDbContext>(opt => opt.UseSqlServer(linuxConn));
}

// Register all application services following SOLID principles
builder.Services.AddApplicationServices();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// --- Connection diagnostics: log masked connection string and test connectivity ---
var startLogger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ShiftsLoggerDbContext>();
    var connString = dbContext.Database.GetDbConnection().ConnectionString ?? "<empty>";
    // mask password for logs
    var masked = Regex.Replace(connString, @"(Password=)([^;]+)", "$1*****", RegexOptions.IgnoreCase);
    startLogger.LogInformation("Resolved DB connection string: {Conn}", masked);

    // quick connectivity check
    if (!dbContext.Database.CanConnect())
    {
        startLogger.LogError("Database connectivity test failed: Cannot connect to the database.");
    }
    else
    {
        startLogger.LogInformation("Database connectivity test succeeded.");
    }
}
catch (Exception ex)
{
    startLogger.LogError(ex, "Exception while testing database connectivity at startup");
}
// --- end diagnostics ---

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Development Mode");

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ShiftsLoggerDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
    var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ShiftsLoggerDbContext>>();
    dbContext.SeedData(logger);
    Console.WriteLine("Database seeded");
}
 
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Shifts Logger API")
        .WithTheme(ScalarTheme.BluePlanet)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .WithModels()
        .WithLayout(ScalarLayout.Classic);
});

app.MapControllers();

app.Run();