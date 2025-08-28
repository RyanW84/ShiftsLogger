using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://127.0.0.1:7009/")
        };

        await TestWorkers(client);
        await TestLocations(client);
    }

    static async Task TestWorkers(HttpClient client)
    {
        try
        {
            Console.WriteLine("=== Testing Workers ===");
            var response = await client.GetAsync("api/workers?pageNumber=1&pageSize=25");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Worker>>>();
            Console.WriteLine($"Total Workers: {result?.TotalCount}");
            Console.WriteLine($"Workers on Page 1: {result?.Data?.Count}");

            // Check for new workers
            var newWorkers = result?.Data?.Where(w => w.Name.Contains("Kevin") || w.Name.Contains("Rachel")).ToList();
            Console.WriteLine($"New Workers Found: {newWorkers?.Count ?? 0}");
            if (newWorkers != null)
            {
                foreach (var worker in newWorkers)
                {
                    Console.WriteLine($"  - {worker.Name} ({worker.Email})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error testing workers: {ex.Message}");
        }
    }

    static async Task TestLocations(HttpClient client)
    {
        try
        {
            Console.WriteLine("\n=== Testing Locations ===");
            var response = await client.GetAsync("api/locations?pageNumber=1&pageSize=25");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<Location>>>();
            Console.WriteLine($"Total Locations: {result?.TotalCount}");
            Console.WriteLine($"Locations on Page 1: {result?.Data?.Count}");

            // Check for new locations
            var newLocations = result?.Data?.Where(l => l.Name.Contains("Edinburgh") || l.Name.Contains("Leicester") || l.Name.Contains("Nottingham")).ToList();
            Console.WriteLine($"New Locations Found: {newLocations?.Count ?? 0}");
            if (newLocations != null)
            {
                foreach (var location in newLocations)
                {
                    Console.WriteLine($"  - {location.Name} ({location.Town})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error testing locations: {ex.Message}");
        }
    }
}

public class ApiResponse<T>
{
    public bool RequestFailed { get; set; }
    public int TotalCount { get; set; }
    public T? Data { get; set; }
}

public class Worker
{
    public int WorkerId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

public class Location
{
    public int LocationId { get; set; }
    public string? Name { get; set; }
    public string? Town { get; set; }
}
