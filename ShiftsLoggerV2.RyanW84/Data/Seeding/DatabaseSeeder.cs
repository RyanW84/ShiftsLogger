using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShiftsLoggerV2.RyanW84.Data;
using ShiftsLoggerV2.RyanW84.Models;

namespace ShiftsLoggerV2.RyanW84.Data.Seeding;

/// <summary>
/// Handles database seeding operations following the Single Responsibility Principle
/// </summary>
public class DatabaseSeeder
{
    private readonly ShiftsLoggerDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ShiftsLoggerDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Seeds all initial data if the database is empty
    /// </summary>
    public void SeedAll()
    {
        SeedWorkers();
        SeedLocations();
        SeedRandomShifts();
    }

    /// <summary>
    /// Seeds worker data
    /// </summary>
    private void SeedWorkers()
    {
        var workerData = new[]
        {
            ("John Smith", "john.smith@company.com", "+44 7911 123456"),
            ("Sarah Johnson", "sarah.johnson@company.com", "+44 7700 900123"),
            ("Mike Davis", "mike.davis@company.com", "+44 7802 345678"),
            ("Emily Wilson", "emily.wilson@company.com", "+44 7920 765432"),
            ("David Brown", "david.brown@company.com", "+44 7555 123456"),
            ("Lisa Anderson", "lisa.anderson@company.com", "+44 7666 789012"),
            ("James Taylor", "james.taylor@company.com", "+44 7777 234567"),
            ("Anna Thompson", "anna.thompson@company.com", "+44 7888 345678"),
            ("Robert Miller", "robert.miller@company.com", "+44 7944 567890"),
            ("Jessica Garcia", "jessica.garcia@company.com", "+44 7712 345678"),
            ("Christopher Martinez", "christopher.martinez@company.com", "+44 7823 456789"),
            ("Amanda Rodriguez", "amanda.rodriguez@company.com", "+44 7956 789012"),
            ("Matthew Hernandez", "matthew.hernandez@company.com", "+44 7734 567890"),
            ("Jennifer Lopez", "jennifer.lopez@company.com", "+44 7867 890123"),
            ("Daniel Gonzalez", "daniel.gonzalez@company.com", "+44 7989 012345"),
            ("Michelle Wilson", "michelle.wilson@company.com", "+44 7745 678901"),
            ("Andrew Moore", "andrew.moore@company.com", "+44 7812 345678"),
            ("Stephanie Taylor", "stephanie.taylor@company.com", "+44 7967 890123"),
            ("Kevin White", "kevin.white@company.com", "+44 7890 123456"),
            ("Rachel Green", "rachel.green@company.com", "+44 7901 234567")
        };

        foreach (var (name, email, phone) in workerData)
        {
            _context.Workers.Add(new Worker
            {
                Name = name,
                Email = email,
                PhoneNumber = phone
            });
        }
        _context.SaveChanges();
        _logger.LogInformation("Seeded {Count} workers", workerData.Length);
    }

    /// <summary>
    /// Seeds location data
    /// </summary>
    private void SeedLocations()
    {
        var locationData = new[]
        {
            ("London Office", "1 Canary Wharf", "London", "Greater London", "E14 5AB", "UK"),
            ("Manchester Warehouse", "22 Trafford Park", "Manchester", "Greater Manchester", "M17 1AB", "UK"),
            ("Birmingham Plant", "15 Aston Road", "Birmingham", "West Midlands", "B6 4DA", "UK"),
            ("Leeds Service Centre", "8 Wellington Place", "Leeds", "West Yorkshire", "LS1 4AP", "UK"),
            ("Bristol Research Lab", "3 Temple Quay", "Bristol", "Bristol", "BS1 6DZ", "UK"),
            ("Glasgow Branch", "45 George Square", "Glasgow", "Scotland", "G2 1DY", "UK"),
            ("Cardiff Hub", "12 Cardiff Bay", "Cardiff", "Wales", "CF10 4PA", "UK"),
            ("Newcastle Distribution Centre", "28 Quayside", "Newcastle upon Tyne", "Tyne and Wear", "NE1 3DX", "UK"),
            ("Sheffield Manufacturing Plant", "15 Meadowhall Road", "Sheffield", "South Yorkshire", "S9 1BW", "UK"),
            ("Liverpool Logistics Hub", "42 Albert Dock", "Liverpool", "Merseyside", "L3 4AF", "UK"),
            ("Brighton Sales Office", "7 Madeira Drive", "Brighton", "East Sussex", "BN2 1PS", "UK"),
            ("Cambridge Research Facility", "19 Science Park", "Cambridge", "Cambridgeshire", "CB4 0EY", "UK"),
            ("Oxford Innovation Centre", "8 Botley Road", "Oxford", "Oxfordshire", "OX2 0HH", "UK"),
            ("Norwich Regional Office", "33 Gurney Road", "Norwich", "Norfolk", "NR1 4HW", "UK"),
            ("Plymouth Marine Services", "11 The Barbican", "Plymouth", "Devon", "PL1 2LS", "UK"),
            ("Exeter Business Park", "25 Matford Business Park", "Exeter", "Devon", "EX2 8ED", "UK"),
            ("Southampton Port Facility", "9 Ocean Gate", "Southampton", "Hampshire", "SO14 3QN", "UK"),
            ("Edinburgh Tech Hub", "5 Charlotte Square", "Edinburgh", "Scotland", "EH2 4DR", "UK"),
            ("Leicester Innovation Centre", "18 Millennium Point", "Leicester", "Leicestershire", "LE1 3RW", "UK"),
            ("Nottingham Digital Campus", "25 Talbot Street", "Nottingham", "Nottinghamshire", "NG1 5GG", "UK")
        };

        foreach (var (name, address, town, county, postCode, country) in locationData)
        {
            _context.Locations.Add(new Location
            {
                Name = name,
                Address = address,
                Town = town,
                County = county,
                PostCode = postCode,
                Country = country
            });
        }
        _context.SaveChanges();
        _logger.LogInformation("Seeded {Count} locations", locationData.Length);
    }

    /// <summary>
    /// Seeds random shifts for testing/demo purposes
    /// </summary>
    private void SeedRandomShifts()
    {
        var savedWorkers = _context.Workers.ToList();
        var savedLocations = _context.Locations.ToList();

        if (savedWorkers.Count == 0 || savedLocations.Count == 0)
        {
            _logger.LogWarning("Cannot seed shifts: No workers or locations available.");
            return;
        }

        var currentShiftCount = _context.Shifts.Count();
        if (currentShiftCount >= 20)
        {
            _logger.LogInformation("Sufficient shifts already exist ({ShiftCount}). Skipping seeding.", currentShiftCount);
            return;
        }

        var shiftsToGenerate = 20 - currentShiftCount;
        var randomShifts = GenerateRandomShifts(savedWorkers, savedLocations, shiftsToGenerate);

        try
        {
            var addedCount = 0;
            foreach (var shift in randomShifts)
            {
                if (shift.EndTime <= shift.StartTime)
                    continue;

                var exists = _context.Shifts.Any(s =>
                    s.WorkerId == shift.WorkerId
                    && s.LocationId == shift.LocationId
                    && s.StartTime == shift.StartTime
                );

                if (!exists)
                {
                    _context.Shifts.Add(shift);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                _context.SaveChanges();
                _logger.LogInformation("Added {AddedCount} random shifts. Total shifts in database: {ShiftCount}",
                    addedCount, _context.Shifts.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding random shifts.");
        }
    }

    /// <summary>
    /// Generates random shifts for seeding
    /// </summary>
    private List<Shift> GenerateRandomShifts(List<Worker> workers, List<Location> locations, int count)
    {
        var shifts = new List<Shift>();
        var random = new Random();
        var baseDate = DateTimeOffset.Now.Date;

        var shiftDurations = new[] { 4, 6, 8, 10, 12 };
        var startHours = new[] { 6, 7, 8, 9, 14, 15, 16, 18, 22 };

        for (int i = 0; i < count; i++)
        {
            var worker = workers[random.Next(workers.Count)];
            var location = locations[random.Next(locations.Count)];

            var daysOffset = random.Next(-15, 16);
            var startHour = startHours[random.Next(startHours.Length)];
            var startTime = baseDate.AddDays(daysOffset).AddHours(startHour);

            var durationHours = shiftDurations[random.Next(shiftDurations.Length)];
            var endTime = startTime.AddHours(durationHours);

            shifts.Add(new Shift
            {
                WorkerId = worker.WorkerId,
                LocationId = location.LocationId,
                StartTime = startTime,
                EndTime = endTime
            });
        }

        return shifts;
    }

    /// <summary>
    /// Clears all shifts for testing purposes
    /// </summary>
    public void ResetShiftsForTesting()
    {
        try
        {
            var shiftCount = _context.Shifts.Count();
            if (shiftCount > 0)
            {
                _context.Shifts.RemoveRange(_context.Shifts);
                _context.SaveChanges();
                _logger.LogInformation("Cleared {ShiftCount} shifts from database for testing purposes.", shiftCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset shifts for testing.");
        }
    }
}
