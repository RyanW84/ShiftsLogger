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
            .WithMany(l => l.Shifts)
            .HasForeignKey(s => s.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder
            .Entity<Shift>()
            .HasOne(s => s.Worker)
            .WithMany(w => w.Shifts)
            .HasForeignKey(s => s.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Worker>().HasIndex(w => w.Email).IsUnique(); // Ensure unique email addresses for workers
        modelBuilder.Entity<Worker>().HasIndex(w => w.PhoneNumber).IsUnique();

        // Performance indexes for frequently queried fields
        modelBuilder.Entity<Shift>().HasIndex(s => s.StartTime); // For date range queries
        modelBuilder.Entity<Shift>().HasIndex(s => s.EndTime); // For date range queries
        modelBuilder.Entity<Shift>().HasIndex(s => s.WorkerId); // For filtering shifts by worker
        modelBuilder.Entity<Shift>().HasIndex(s => s.LocationId); // For filtering shifts by location
        modelBuilder.Entity<Shift>().HasIndex(s => new { s.WorkerId, s.StartTime }); // Composite index for worker + date queries
        modelBuilder.Entity<Shift>().HasIndex(s => new { s.LocationId, s.StartTime }); // Composite index for location + date queries

        modelBuilder.Entity<Worker>().HasIndex(w => w.Name); // For searching workers by name

        modelBuilder.Entity<Location>().HasIndex(l => l.Name); // For searching locations by name
        modelBuilder.Entity<Location>().HasIndex(l => l.Town); // For searching locations by town
        modelBuilder.Entity<Location>().HasIndex(l => l.PostCode); // For searching locations by postcode
    }

    public void SeedData(ILogger<ShiftsLoggerDbContext>? logger)
    {
        // Seed workers and locations if they don't exist
        if (!Workers.Any())
        {
            SeedWorkers(logger);
        }

        if (!Locations.Any())
        {
            SeedLocations(logger);
        }

        // Always try to seed some varied duration shifts for testing
        SeedVariedDurationShifts(logger);
    }

    private void SeedWorkers(ILogger<ShiftsLoggerDbContext>? logger)
    {
        var workers = new List<Worker>
        {
            new()
            {
                Name = "John Smith",
                Email = "john.smith@company.com",
                PhoneNumber = "+44 7911 123456",
            },
            new()
            {
                Name = "Sarah Johnson",
                Email = "sarah.johnson@company.com",
                PhoneNumber = "+44 7700 900123",
            },
            new()
            {
                Name = "Mike Davis",
                Email = "mike.davis@company.com",
                PhoneNumber = "+44 7802 345678",
            },
            new()
            {
                Name = "Emily Wilson",
                Email = "emily.wilson@company.com",
                PhoneNumber = "+44 7920 765432",
            },
            new()
            {
                Name = "David Brown",
                Email = "david.brown@company.com",
                PhoneNumber = "+44 7555 123456",
            },
        };

        foreach (var w in workers)
        {
            if (!string.IsNullOrWhiteSpace(w.Email) && !Workers.Any(x => x.Email != null && x.Email.ToLower() == w.Email.ToLower()))
                Workers.Add(w);
        }
        SaveChanges();
    }

    private void SeedLocations(ILogger<ShiftsLoggerDbContext>? logger)
    {
        var locations = new List<Location>
        {
            new()
            {
                Name = "London Office",
                Address = "1 Canary Wharf",
                Town = "London",
                County = "Greater London",
                PostCode = "E14 5AB",
                Country = "UK",
            },
            new()
            {
                Name = "Manchester Warehouse",
                Address = "22 Trafford Park",
                Town = "Manchester",
                County = "Greater Manchester",
                PostCode = "M17 1AB",
                Country = "UK",
            },
            new()
            {
                Name = "Birmingham Plant",
                Address = "15 Aston Road",
                Town = "Birmingham",
                County = "West Midlands",
                PostCode = "B6 4DA",
                Country = "UK",
            },
            new()
            {
                Name = "Leeds Service Centre",
                Address = "8 Wellington Place",
                Town = "Leeds",
                County = "West Yorkshire",
                PostCode = "LS1 4AP",
                Country = "UK",
            },
            new()
            {
                Name = "Bristol Research Lab",
                Address = "3 Temple Quay",
                Town = "Bristol",
                County = "Bristol",
                PostCode = "BS1 6DZ",
                Country = "UK",
            },
        };

        foreach (var l in locations)
        {
            if (!string.IsNullOrWhiteSpace(l.Name) && !Locations.Any(x => x.Name.ToLower() == l.Name.ToLower()))
                Locations.Add(l);
        }
        SaveChanges();
    }

    private void SeedVariedDurationShifts(ILogger<ShiftsLoggerDbContext>? logger)
    {
        var savedWorkers = Workers.ToList();
        var savedLocations = Locations.ToList();

        if (!savedWorkers.Any() || !savedLocations.Any())
            return;

        // Seed shifts with varied durations to test duration filtering and display
        var variedShifts = new List<Shift>
        {
            new()
            {
                StartTime = DateTimeOffset.Now.AddDays(3).AddHours(6),
                EndTime = DateTimeOffset.Now.AddDays(3).AddHours(10), // 4 hours
                WorkerId = savedWorkers.First().WorkerId,
                LocationId = savedLocations.Skip(1).FirstOrDefault()?.LocationId ?? savedLocations.First().LocationId,
            },
            new()
            {
                StartTime = DateTimeOffset.Now.AddDays(4).AddHours(14),
                EndTime = DateTimeOffset.Now.AddDays(4).AddHours(22), // 8 hours
                WorkerId = savedWorkers.Skip(1).FirstOrDefault()?.WorkerId ?? savedWorkers.First().WorkerId,
                LocationId = savedLocations.Skip(2).FirstOrDefault()?.LocationId ?? savedLocations.First().LocationId,
            },
            new()
            {
                StartTime = DateTimeOffset.Now.AddDays(5).AddHours(9),
                EndTime = DateTimeOffset.Now.AddDays(5).AddHours(14), // 5 hours
                WorkerId = savedWorkers.Skip(2).FirstOrDefault()?.WorkerId ?? savedWorkers.First().WorkerId,
                LocationId = savedLocations.Skip(3).FirstOrDefault()?.LocationId ?? savedLocations.First().LocationId,
            },
            new()
            {
                StartTime = DateTimeOffset.Now.AddDays(6).AddHours(16),
                EndTime = DateTimeOffset.Now.AddDays(6).AddHours(20), // 4 hours
                WorkerId = savedWorkers.Skip(3).FirstOrDefault()?.WorkerId ?? savedWorkers.First().WorkerId,
                LocationId = savedLocations.Skip(4).FirstOrDefault()?.LocationId ?? savedLocations.First().LocationId,
            },
            new()
            {
                StartTime = DateTimeOffset.Now.AddDays(7).AddHours(8),
                EndTime = DateTimeOffset.Now.AddDays(7).AddHours(12), // 4 hours
                WorkerId = savedWorkers.Skip(4).FirstOrDefault()?.WorkerId ?? savedWorkers.First().WorkerId,
                LocationId = savedLocations.First().LocationId,
            },
            new()
            {
                StartTime = DateTimeOffset.Now.AddDays(8).AddHours(10),
                EndTime = DateTimeOffset.Now.AddDays(8).AddHours(18), // 8 hours
                WorkerId = savedWorkers.First().WorkerId,
                LocationId = savedLocations.Skip(1).FirstOrDefault()?.LocationId ?? savedLocations.First().LocationId,
            },
            new()
            {
                StartTime = DateTimeOffset.Now.AddDays(9).AddHours(13),
                EndTime = DateTimeOffset.Now.AddDays(9).AddHours(21), // 8 hours
                WorkerId = savedWorkers.Skip(1).FirstOrDefault()?.WorkerId ?? savedWorkers.First().WorkerId,
                LocationId = savedLocations.Skip(2).FirstOrDefault()?.LocationId ?? savedLocations.First().LocationId,
            },
            new()
            {
                StartTime = DateTimeOffset.Now.AddDays(10).AddHours(11),
                EndTime = DateTimeOffset.Now.AddDays(10).AddHours(15), // 4 hours
                WorkerId = savedWorkers.Skip(2).FirstOrDefault()?.WorkerId ?? savedWorkers.First().WorkerId,
                LocationId = savedLocations.Skip(3).FirstOrDefault()?.LocationId ?? savedLocations.First().LocationId,
            },
        };

        try
        {
            // Only add shifts that don't already exist (by StartTime, WorkerId, LocationId)
            foreach (var s in variedShifts)
            {
                if (s.EndTime <= s.StartTime)
                    continue; // skip invalid

                var exists = Shifts.Any(x =>
                    x.WorkerId == s.WorkerId
                    && x.LocationId == s.LocationId
                    && x.StartTime == s.StartTime
                );
                if (!exists)
                    Shifts.Add(s);
            }

            SaveChanges();

            // Log summary
            try
            {
                logger?.LogInformation(
                    "Added varied duration shifts. Total shifts in database: {ShiftCount}",
                    Shifts.Count()
                );
            }
            catch (Exception logEx)
            {
                try
                {
                    Console.WriteLine($"Seeding summary log error: {logEx}");
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            // Use provided logger to log exceptions during seeding
            try
            {
                logger?.LogError(ex, "An error occurred while seeding varied duration shifts.");
            }
            catch
            {
                try
                {
                    Console.WriteLine($"Seeding error: {ex}");
                }
                catch { }
            }
        }
    }
}
