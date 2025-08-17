using Microsoft.EntityFrameworkCore;
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
            .WithMany()
            .HasForeignKey(s => s.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder
            .Entity<Shift>()
            .HasOne(s => s.Worker)
            .WithMany()
            .HasForeignKey(s => s.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder
            .Entity<Worker>()
            .HasIndex(w => w.Email)
            .IsUnique(); // Ensure unique email addresses for workers
        modelBuilder
            .Entity<Worker>()
            .HasIndex(w => w.PhoneNumber)
            .IsUnique();
    }

    public void SeedData()
    {
        // Only seed data if tables are empty
        if (Workers.Any() || Locations.Any() || Shifts.Any())
            return;

        // Seed Workers
        var workers = new List<Worker>
        {
            new Worker
            {
                Name = "John Smith",
                Email = "john.smith@company.com",
                PhoneNumber = "+44 7911 123456"
            },
            new Worker
            {
                Name = "Sarah Johnson",
                Email = "sarah.johnson@company.com",
                PhoneNumber = "+44 7700 900123"
            },
            new Worker
            {
                Name = "Mike Davis",
                Email = "mike.davis@company.com",
                PhoneNumber = "+44 7802 345678"
            },
            new Worker
            {
                Name = "Emily Wilson",
                Email = "emily.wilson@company.com",
                PhoneNumber = "+44 7920 765432"
            },
            new Worker
            {
                Name = "David Brown",
                Email = "david.brown@company.com",
                PhoneNumber = "+44 7555 123456"
            }
        };

        // Seed Locations
        var locations = new List<Location>
        {
            new Location
            {
                Name = "London Office",
                Address = "1 Canary Wharf",
                Town = "London",
                County = "Greater London",
                PostCode = "E14 5AB",
                Country = "UK"
            },
            new Location
            {
                Name = "Manchester Warehouse",
                Address = "22 Trafford Park",
                Town = "Manchester",
                County = "Greater Manchester",
                PostCode = "M17 1AB",
                Country = "UK"
            },
            new Location
            {
                Name = "Birmingham Plant",
                Address = "15 Aston Road",
                Town = "Birmingham",
                County = "West Midlands",
                PostCode = "B6 4DA",
                Country = "UK"
            },
            new Location
            {
                Name = "Leeds Service Centre",
                Address = "8 Wellington Place",
                Town = "Leeds",
                County = "West Yorkshire",
                PostCode = "LS1 4AP",
                Country = "UK"
            },
            new Location
            {
                Name = "Bristol Research Lab",
                Address = "3 Temple Quay",
                Town = "Bristol",
                County = "Bristol",
                PostCode = "BS1 6DZ",
                Country = "UK"
            }
        };

        Workers.AddRange(workers);
        Locations.AddRange(locations);
        SaveChanges();

        // Get the saved entities with their IDs
        var savedWorkers = Workers.ToList();
        var savedLocations = Locations.ToList();

        // Seed Shifts (using the actual IDs from saved entities)
        var shifts = new List<Shift>
        {
            new Shift
            {
                StartTime = DateTimeOffset.Now.AddHours(-2),
                EndTime = DateTimeOffset.Now.AddHours(6),
                WorkerId = savedWorkers[0].WorkerId,
                LocationId = savedLocations[0].LocationId
            },
            new Shift
            {
                StartTime = DateTimeOffset.Now.AddDays(1).AddHours(8),
                EndTime = DateTimeOffset.Now.AddDays(1).AddHours(16),
                WorkerId = savedWorkers[1].WorkerId,
                LocationId = savedLocations[1].LocationId
            },
            new Shift
            {
                StartTime = DateTimeOffset.Now.AddDays(2).AddHours(7),
                EndTime = DateTimeOffset.Now.AddDays(2).AddHours(15),
                WorkerId = savedWorkers[2].WorkerId,
                LocationId = savedLocations[2].LocationId
            },
            new Shift
            {
                StartTime = DateTimeOffset.Now.AddDays(-1).AddHours(9),
                EndTime = DateTimeOffset.Now.AddDays(-1).AddHours(17),
                WorkerId = savedWorkers[3].WorkerId,
                LocationId = savedLocations[3].LocationId
            },
            new Shift
            {
                StartTime = DateTimeOffset.Now.AddDays(-2).AddHours(10),
                EndTime = DateTimeOffset.Now.AddDays(-2).AddHours(18),
                WorkerId = savedWorkers[4].WorkerId,
                LocationId = savedLocations[4].LocationId
            }
        };

        Shifts.AddRange(shifts);
        SaveChanges();
    }
}