using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShiftsLoggerV2.RyanW84.Models;

namespace ShiftsLoggerV2.RyanW84.Data;

public class ShiftsLoggerDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Shift> Shifts { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Worker> Workers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Shift>()
            .HasOne(s => s.Location)
            .WithMany(l => l.Shifts)
            .HasForeignKey(s => s.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder
            .Entity<Shift>()
            .HasOne(s => s.Worker)
            .WithMany(w => w.Shifts)
            .HasForeignKey(s => s.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Worker>().HasIndex(w => w.Email).IsUnique();
        modelBuilder.Entity<Worker>().HasIndex(w => w.PhoneNumber).IsUnique();

        // Performance indexes for frequently queried fields
        modelBuilder.Entity<Shift>().HasIndex(s => s.StartTime);
        modelBuilder.Entity<Shift>().HasIndex(s => s.EndTime);
        modelBuilder.Entity<Shift>().HasIndex(s => s.WorkerId);
        modelBuilder.Entity<Shift>().HasIndex(s => s.LocationId);
        modelBuilder.Entity<Shift>().HasIndex(s => new { s.WorkerId, s.StartTime });
        modelBuilder.Entity<Shift>().HasIndex(s => new { s.LocationId, s.StartTime });

        modelBuilder.Entity<Worker>().HasIndex(w => w.Name);

        modelBuilder.Entity<Location>().HasIndex(l => l.Name);
        modelBuilder.Entity<Location>().HasIndex(l => l.Town);
        modelBuilder.Entity<Location>().HasIndex(l => l.PostCode);
    }

    public void SeedData(ILogger<ShiftsLoggerDbContext>? logger)
    {
        logger?.LogInformation("Starting database seeding process...");

        // Delete existing database
        logger?.LogInformation("Deleting existing database...");
        Database.EnsureDeleted();
        logger?.LogInformation("Database deleted successfully.");

        // Create new database
        logger?.LogInformation("Creating new database...");
        Database.EnsureCreated();
        logger?.LogInformation("Database created successfully.");

        // Seed the data
        logger?.LogInformation("Seeding data...");
        var seederLogger = logger != null
            ? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Seeding.DatabaseSeeder>()
            : LoggerFactory.Create(builder => {}).CreateLogger<Seeding.DatabaseSeeder>();
        var seeder = new Seeding.DatabaseSeeder(this, seederLogger);
        seeder.SeedAll();

        logger?.LogInformation("Database seeding process completed successfully.");
    }










}