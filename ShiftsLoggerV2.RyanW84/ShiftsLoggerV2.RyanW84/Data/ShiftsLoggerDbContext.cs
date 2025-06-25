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
            .HasIndex(w=> w.PhoneNumber)
            .IsUnique();

	}

    public void SeedData()
    {
        Shifts.RemoveRange(Shifts); // ? Cascade delete ?
        Locations.RemoveRange(Locations);
        Workers.RemoveRange(Workers);

        var locations = new List<Location>
        {
            new Location
            {
                Name = "Colchester General Hospital",
                Address = "Turner Road",
                Town = "Colchester",
                Country = "Essex",
                PostCode = "CO4 5JL",
                County = "England",
            },
            new Location
            {
                Name = "The Royal Brisbane and Women's Hospital",
                Address = "Butterfield Street",
                Town = "Herston",
                County = "Queensland",
                PostCode = "QLD 4006",
                Country = "Australia",
            },
            new Location
            {
                Name = "Advent Health",
                Address = "601 E Rollins Street",
                Town = "Orlando",
                County = "Florida",
                PostCode = "FL 32803",
                Country = "USA",
            },
        };

        Locations.AddRange(locations);

        var workers = new List<Worker>
        {
            new Worker
            {
                Name = "John Doe",
                PhoneNumber = "123-456-7890",
                Email = "John@Doe.com",
            },
            new Worker
            {
                Name = "Jane Doe",
                PhoneNumber = "123-456-7892",
                Email = "Jane@Doe.com",
            },
            new Worker
            {
                Name = "Jim Doe",
                PhoneNumber = "123-456-7893",
                Email = "Jim@yahoo.com",
            },
        };
        Workers.AddRange(workers);

        // Save changes to generate IDs
        SaveChanges();

        // Retrieve the generated WorkerIds
        var workerIds = Workers.Select(w => w.WorkerId).ToList();

        Shifts.AddRange(
            new Shift
            {
                WorkerId = workerIds[0],
                StartTime = DateTimeOffset.UtcNow.AddHours(2),
                EndTime = DateTimeOffset.UtcNow.AddHours(10),
                Location = locations[0],
            },
            new Shift
            {
                WorkerId = workerIds[1],
                StartTime = DateTimeOffset.UtcNow.AddHours(1),
                EndTime = DateTimeOffset.UtcNow.AddHours(5),
                Location = locations[1],
            },
            new Shift
            {
                WorkerId = workerIds[2],
                StartTime = DateTimeOffset.UtcNow.AddHours(3),
                EndTime = DateTimeOffset.UtcNow.AddHours(8),
                Location = locations[2],
            }
        );

        SaveChanges();
    }
}
