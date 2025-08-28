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

        try
        {
            Console.WriteLine("Testing Workers API with pagination...");
            var response = await client.GetAsync("api/workers?pageNumber=1&pageSize=10");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API Response:");
            Console.WriteLine(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
