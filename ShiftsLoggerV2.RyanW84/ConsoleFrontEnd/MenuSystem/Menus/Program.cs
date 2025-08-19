using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Prevents circular dependency issues

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opts.JsonSerializerOptions.Converters.Add(new DdMmYyyyHHmmDateTimeOffsetConverter());
});

builder.Services.AddDbContext<ShiftsLoggerDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Register all application services following SOLID principles
builder.Services.AddApplicationServices();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();
// Custom DateTimeOffset converter for dd-MM-yyyy HH:mm
public class DdMmYyyyHHmmDateTimeOffsetConverter : System.Text.Json.Serialization.JsonConverter<DateTimeOffset>
{
    private const string Format = "dd-MM-yyyy HH:mm";
    public override DateTimeOffset Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (DateTimeOffset.TryParseExact(str, Format, null, System.Globalization.DateTimeStyles.None, out var dto))
            return dto;
        throw new JsonException($"Invalid date format. Expected {Format}.");
    }
    public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTimeOffset value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}

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