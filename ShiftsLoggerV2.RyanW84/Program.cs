using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.IO;
using Scalar.AspNetCore;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Extensions;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Prevents circular dependency issues
builder
    .Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
    );

// Configure DbContext provider based on OS: use LocalDB on Windows, SQLite file on Linux/macOS
var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
if (isWindows)
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection")
               ?? "Server=(localdb)\\MSSQLLocalDB;Database=ShiftsLoggerDb;Trusted_Connection=True;MultipleActiveResultSets=true";
    builder.Services.AddDbContext<ShiftsLoggerDbContext>(opt => opt.UseSqlServer(conn));
}
else
{
    // On non-Windows platforms, use SQL Server (not LocalDB). Prefer:
    // 1) connection string named 'DefaultConnection' from configuration
    // 2) environment variable 'SQLSERVER_CONNECTION'
    // 3) a sensible localhost fallback (SQL Server on 127.0.0.1:1433 using sa)
    string? defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
    // If the configured DefaultConnection references LocalDB, ignore it on non-Windows platforms
    if (!string.IsNullOrWhiteSpace(defaultConn) && (defaultConn.Contains("localdb", StringComparison.OrdinalIgnoreCase) || defaultConn.Contains("MSSQLLocalDB", StringComparison.OrdinalIgnoreCase)))
    {
        defaultConn = null;
    }
    string? envConn = Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION");
    string? sqlConn = defaultConn;
    if (string.IsNullOrWhiteSpace(sqlConn)) sqlConn = envConn;

    // If no full connection string provided, assemble one from secret/config values or env vars.
    if (string.IsNullOrWhiteSpace(sqlConn))
    {
        // Prefer configuration keys (user secrets in development) then environment variables.
    string? dbHost = builder.Configuration["Db:Host"] ?? Environment.GetEnvironmentVariable("DB_HOST") ?? "127.0.0.1";
    string? dbPort = builder.Configuration["Db:Port"] ?? Environment.GetEnvironmentVariable("DB_PORT") ?? "1433";
    string? dbName = builder.Configuration["Db:Database"] ?? Environment.GetEnvironmentVariable("DB_DATABASE") ?? "ShiftsLoggerDb";
    string? dbUser = builder.Configuration["Db:User"] ?? Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
    string? dbPass = builder.Configuration["Db:Password"] ?? Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "Your_password123";

        // Build a SQL Server connection string without logging any secrets.
        sqlConn = $"Server={dbHost},{dbPort};User Id={dbUser};Password={dbPass};Database={dbName};TrustServerCertificate=true;MultipleActiveResultSets=true";
    }

    // If a SQL Server connection string is provided/assembled, try to use it; if it's not reachable, fall back to SQLite for developer convenience.
    bool registeredSqlServer = false;
    try
    {
        // If sqlConn was assembled from parts we have host/port, otherwise attempt a short test open using SqlClient.
        if (string.IsNullOrWhiteSpace(sqlConn))
        {
            // no SQL Server connection determined; fall back to SQLite
            throw new InvalidOperationException("No SQL Server connection string available");
        }

        // Quick reachability test for typical assembled connection string containing host and port
        if (sqlConn.Contains(","))
        {
            // Try to extract host,port from the beginning of the connection string 'Server=host,port;...'
            var serverToken = "Server=";
            var start = sqlConn.IndexOf(serverToken, StringComparison.OrdinalIgnoreCase);
            if (start >= 0)
            {
                start += serverToken.Length;
                var end = sqlConn.IndexOf(';', start);
                var hostPort = end > start ? sqlConn[start..end] : sqlConn[start..];
                var parts = hostPort.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && int.TryParse(parts[1], out var port))
                {
                    var host = parts[0].Trim();
                    using var tcp = new System.Net.Sockets.TcpClient();
                    var connectTask = tcp.ConnectAsync(host, port);
                    if (!connectTask.Wait(TimeSpan.FromSeconds(2)))
                    {
                        throw new InvalidOperationException("SQL Server not reachable at configured host/port");
                    }
                }
            }
        }

        // Register SQL Server if we got here
        builder.Services.AddDbContext<ShiftsLoggerDbContext>(opt =>
        {
            opt.UseSqlServer(sqlConn, sqlOpt => sqlOpt.EnableRetryOnFailure());
        });
        registeredSqlServer = true;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"SQL Server not usable: {ex.Message}. On non-Windows platforms the application requires SQL Server (not LocalDB).\nSet a valid 'DefaultConnection' or environment variable 'SQLSERVER_CONNECTION' or configure 'Db:Host','Db:Port','Db:User','Db:Password' secrets.");
        // Fail fast to avoid running in an unexpected state.
        Environment.Exit(1);
    }
}

// Register all application services following SOLID principles
builder.Services.AddApplicationServices();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Before starting, check that the configured URL is available to avoid a hard crash
// when the port is already in use. We read ASPNETCORE_URLS (or use default) and
// probe the first URL's host/port.
string? urlsConfig = builder.Configuration["ASPNETCORE_URLS"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
string probeUrl = urlsConfig ?? "https://127.0.0.1:7009";
try
{
    // Ensure we have an absolute URI
    if (!Uri.TryCreate(probeUrl, UriKind.Absolute, out var probeUri))
    {
        // Try to prefix with https if missing
        Uri.TryCreate($"https://{probeUrl}", UriKind.Absolute, out probeUri);
    }

    if (probeUri != null)
    {
        var host = probeUri.Host;
        var port = probeUri.Port;
        try
        {
            // Attempt to bind a TcpListener briefly to test availability
            var ip = System.Net.IPAddress.None;
            if (System.Net.IPAddress.TryParse(host, out var parsed))
                ip = parsed;
            else if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
                ip = System.Net.IPAddress.Loopback;
            else if (string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase))
                ip = System.Net.IPAddress.Loopback;

            if (ip != System.Net.IPAddress.None)
            {
                var listener = new System.Net.Sockets.TcpListener(ip, port);
                try
                {
                    listener.Start();
                    listener.Stop();
                }
                catch (System.Net.Sockets.SocketException)
                {
                    Console.Error.WriteLine($"Port {port} on {host} appears to be in use. " +
                                            "Ensure no other instance is running or change the URL/port.");
                    Environment.Exit(1);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: failed to probe {probeUrl} for availability: {ex.Message}");
            // continue to attempt startup; the runtime will report any binding errors
        }
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Unexpected error while checking endpoint availability: {ex.Message}");
}

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