using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Prevents circular dependency issues
builder
    .Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler
            .IgnoreCycles
    );

builder.Services.AddDbContext<ShiftsLoggerDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Register all application services following SOLID principles
builder.Services.AddApplicationServices();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Development Mode");

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ShiftsLoggerDbContext>();
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
    dbContext.SeedData();
    Console.WriteLine("Database seeded");
}


// Global exception handler middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (DbUpdateException dbEx)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { error = "Database update error", details = dbEx.Message });
    }
    catch (KeyNotFoundException notFoundEx)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new { error = "Resource not found", details = notFoundEx.Message });
    }
    catch (UnauthorizedAccessException unauthEx)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new { error = "Unauthorized", details = unauthEx.Message });
    }
    catch (AccessViolationException accessEx)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { error = "Forbidden", details = accessEx.Message });
    }
    catch (System.ComponentModel.DataAnnotations.ValidationException validationEx)
    {
        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        await context.Response.WriteAsJsonAsync(new { error = "Validation error", details = validationEx.Message });
    }
    catch (ArgumentException argEx)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { error = "Bad request", details = argEx.Message });
    }
    catch (InvalidOperationException invalidOpEx)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { error = "Invalid operation", details = invalidOpEx.Message });
    }
    catch (TimeoutException timeoutEx)
    {
        context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
        await context.Response.WriteAsJsonAsync(new { error = "Request timed out", details = timeoutEx.Message });
    }
    catch (NullReferenceException nullRefEx)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { error = "Null reference error", details = nullRefEx.Message });
    }
    catch (TaskCanceledException taskCanceledEx)
    {
        context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest; // Non-standard, but used by some proxies
        await context.Response.WriteAsJsonAsync(new { error = "Request was cancelled", details = taskCanceledEx.Message });
    }
    catch (FormatException formatEx)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { error = "Format error", details = formatEx.Message });
    }
    catch (NotImplementedException notImplEx)
    {
        context.Response.StatusCode = StatusCodes.Status501NotImplemented;
        await context.Response.WriteAsJsonAsync(new { error = "Not implemented", details = notImplEx.Message });
    }
    catch (StackOverflowException stackEx)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { error = "Stack overflow error", details = stackEx.Message });
    }
    catch (OutOfMemoryException oomEx)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { error = "Out of memory", details = oomEx.Message });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred", details = ex.Message });
    }
});

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