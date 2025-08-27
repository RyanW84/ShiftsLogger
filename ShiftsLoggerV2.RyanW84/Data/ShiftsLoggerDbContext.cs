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

        // Only seed if we have fewer than 20 shifts total
        var currentShiftCount = Shifts.Count();
        if (currentShiftCount >= 20)
        {
            logger?.LogInformation("Sufficient shifts already exist ({ShiftCount}). Skipping seeding.", currentShiftCount);
            return;
        }

        // Generate a reasonable number of random shifts (not too many to avoid buffer issues)
        var shiftsToGenerate = Math.Min(12, 20 - currentShiftCount);
        var variedShifts = GenerateRandomShifts(savedWorkers, savedLocations, shiftsToGenerate);

        try
        {
            var addedCount = 0;
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
                {
                    Shifts.Add(s);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                SaveChanges();

                // Log summary
                try
                {
                    logger?.LogInformation(
                        "Added {AddedCount} varied duration shifts. Total shifts in database: {ShiftCount}",
                        addedCount, Shifts.Count()
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

    private List<Shift> GenerateRandomShifts(List<Worker> workers, List<Location> locations, int count)
    {
        var shifts = new List<Shift>();
        var random = new Random();
        var baseDate = DateTimeOffset.Now;

        // Possible shift durations in hours
        var durations = new[] { 4, 5, 8 };

        for (int i = 0; i < count; i++)
        {
            // Select random worker and location
            var worker = workers[random.Next(workers.Count)];
            var location = locations[random.Next(locations.Count)];

            // Generate random start time (between -2 days and +10 days from now)
            var daysOffset = random.Next(-2, 11);
            var hoursOffset = random.Next(6, 22); // Business hours: 6 AM to 10 PM
            var startTime = baseDate.AddDays(daysOffset).AddHours(hoursOffset);

            // Select random duration
            var durationHours = durations[random.Next(durations.Length)];
            var endTime = startTime.AddHours(durationHours);

            // Ensure end time is not in the past if start time is in the past
            if (endTime < baseDate && startTime < baseDate)
            {
                // If both times are in the past, move them to future
                var futureOffset = random.Next(1, 8);
                startTime = baseDate.AddDays(futureOffset).AddHours(hoursOffset);
                endTime = startTime.AddHours(durationHours);
            }

            var shift = new Shift
            {
                WorkerId = worker.WorkerId,
                LocationId = location.LocationId,
                StartTime = startTime,
                EndTime = endTime
            };

            shifts.Add(shift);
        }

        return shifts;
    }

    public void ResetShiftsForTesting(ILogger<ShiftsLoggerDbContext>? logger)
    {
        try
        {
            var shiftCount = Shifts.Count();
            if (shiftCount > 0)
            {
                Shifts.RemoveRange(Shifts);
                SaveChanges();
                logger?.LogInformation("Cleared {ShiftCount} shifts from database for testing purposes.", shiftCount);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to reset shifts for testing.");
        }
    }
}
