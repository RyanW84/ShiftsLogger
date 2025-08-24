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

        // Seed Workers (idempotent: skip if worker with same email exists)
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

        // Seed Locations (idempotent: skip if location with same name exists)
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

        try
        {
            // Insert workers if they do not already exist (by email)
            foreach (var w in workers)
            {
                if (string.IsNullOrWhiteSpace(w.Email))
                    continue;
                if (!Workers.Any(x => x.Email != null && x.Email.ToLower() == w.Email.ToLower()))
                    Workers.Add(w);
            }

            // Insert locations if not already present (by name)
            foreach (var l in locations)
            {
                if (string.IsNullOrWhiteSpace(l.Name))
                    continue;
                if (!Locations.Any(x => x.Name.ToLower() == l.Name.ToLower()))
                    Locations.Add(l);
            }

            SaveChanges();

            // Get the saved entities with their IDs
            var savedWorkers = Workers.ToList();
            var savedLocations = Locations.ToList();

            // Seed Shifts (using the actual IDs from saved entities)
            // Seed Shifts (ensure referenced worker/location IDs exist and times are sensible)
            var shifts = new List<Shift>
        {
            new Shift
            {
                    StartTime = DateTimeOffset.Now.AddHours(-2),
                    EndTime = DateTimeOffset.Now.AddHours(6),
                    WorkerId = savedWorkers.First().WorkerId,
                    LocationId = savedLocations.First().LocationId
            },
            new Shift
            {
                    StartTime = DateTimeOffset.Now.AddDays(1).AddHours(8),
                    EndTime = DateTimeOffset.Now.AddDays(1).AddHours(16),
                    WorkerId = savedWorkers.Skip(1).FirstOrDefault()?.WorkerId ?? savedWorkers.First().WorkerId,
                    LocationId = savedLocations.Skip(1).FirstOrDefault()?.LocationId ?? savedLocations.First().LocationId
            },
            new Shift
            {
                    StartTime = DateTimeOffset.Now.AddDays(2).AddHours(7),
                    EndTime = DateTimeOffset.Now.AddDays(2).AddHours(15),
                    WorkerId = savedWorkers.Skip(2).FirstOrDefault()?.WorkerId ?? savedWorkers.First().WorkerId,
                    LocationId = savedLocations.Skip(2).FirstOrDefault()?.LocationId ?? savedLocations.First().LocationId
            },
            new Shift
            {
                    StartTime = DateTimeOffset.Now.AddDays(-1).AddHours(9),
                    EndTime = DateTimeOffset.Now.AddDays(-1).AddHours(17),
                    WorkerId = savedWorkers.Skip(3).FirstOrDefault()?.WorkerId ?? savedWorkers.First().WorkerId,
                    LocationId = savedLocations.Skip(3).FirstOrDefault()?.LocationId ?? savedLocations.First().LocationId
            },
            new Shift
            {
                    StartTime = DateTimeOffset.Now.AddDays(-2).AddHours(10),
                    EndTime = DateTimeOffset.Now.AddDays(-2).AddHours(18),
                    WorkerId = savedWorkers.Skip(4).FirstOrDefault()?.WorkerId ?? savedWorkers.First().WorkerId,
                    LocationId = savedLocations.Skip(4).FirstOrDefault()?.LocationId ?? savedLocations.First().LocationId
            }
        };
            // Only add shifts that don't already exist (by StartTime, WorkerId, LocationId)
            foreach (var s in shifts)
            {
                if (s.EndTime <= s.StartTime)
                    continue; // skip invalid

                var exists = Shifts.Any(x => x.WorkerId == s.WorkerId && x.LocationId == s.LocationId && x.StartTime == s.StartTime);
                if (!exists)
                    Shifts.Add(s);
            }

            SaveChanges();
        }
        catch (Exception)
        {
            // Swallow exceptions during seeding to avoid crashing dev startup; real apps should log this properly
        }
    }
}