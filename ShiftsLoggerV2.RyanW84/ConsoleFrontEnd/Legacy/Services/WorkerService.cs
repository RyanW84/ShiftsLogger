using System.Net;
using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using Spectre.Console;

namespace ConsoleFrontEnd.Services;

public class WorkerService : IWorkerService
{
    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5181/")
    };

    public async Task<ApiResponseDto<List<Worker>>> GetAllWorkers(
        WorkerFilterOptions workerFilterOptions
    )
    {
        try
        {
            var queryString = BuildQueryString("api/workers", workerFilterOptions);

            AnsiConsole.MarkupLine(
                $"[blue]Final request URL: {httpClient.BaseAddress}{queryString}[/]\n"
            );

            var response = await httpClient.GetAsync(queryString);
            if (!response.IsSuccessStatusCode)
            {
                AnsiConsole.Markup("[red]Workers not retrieved.[/]\n");
                return new ApiResponseDto<List<Worker>>(response.ReasonPhrase ?? "Unknown error")
                {
                    ResponseCode = response.StatusCode,
                    Data = null
                };
            }

            AnsiConsole.Markup("[green]Workers retrieved successfully.[/]\n");
            return await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Worker>>>()
                   ?? new ApiResponseDto<List<Worker>>
                   {
                       ResponseCode = response.StatusCode,
                       Message = "Data obtained",
                       Data = new List<Worker>()
                   };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for GetAllWorkers: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Worker>> GetWorkerById(int id)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"api/workers/{id}");

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>()
                       ?? new ApiResponseDto<Worker>
                       {
                           ResponseCode = response.StatusCode,
                           Message = "No data returned.",
                           Data = response.Content.ReadFromJsonAsync<Worker>().Result,
                           TotalCount = 0
                       };

            return await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>()
                   ?? new ApiResponseDto<Worker>
                   {
                       ResponseCode = response.StatusCode,
                       Message = "No data returned.",
                       Data = null,
                       TotalCount = 0
                   };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for GetWorkerById: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Worker>> CreateWorker(Worker createdWorker)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/workers", createdWorker);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                Console.WriteLine($"Error: Status Code - {response.StatusCode}");
                return new ApiResponseDto<Worker>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase ?? "Creation failed",
                    Data = null
                };
            }

            Console.WriteLine("Worker created successfully.");
            return new ApiResponseDto<Worker>
            {
                ResponseCode = response.StatusCode,
                Data = await response.Content.ReadFromJsonAsync<Worker>() ?? createdWorker
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for CreateWorker: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Worker>> UpdateWorker(int id, Worker updatedWorker)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync($"api/workers/{id}", updatedWorker);
            if (response.StatusCode is not HttpStatusCode.OK)
                return new ApiResponseDto<Worker>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase,
                    Data = null
                };

            return await response.Content.ReadFromJsonAsync<ApiResponseDto<Worker>>()
                   ?? new ApiResponseDto<Worker>
                   {
                       ResponseCode = response.StatusCode,
                       Message = "Update Worker succeeded.",
                       Data = null
                   };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for UpdateWorker: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<string>> DeleteWorker(int id)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/workers/{id}");
            if (response.StatusCode != HttpStatusCode.NoContent)
                return new ApiResponseDto<string>
                {
                    ResponseCode = response.StatusCode,
                    Message = $"Error deleting Worker - {response.StatusCode}",
                    Data = null
                };

            return new ApiResponseDto<string>
            {
                ResponseCode = response.StatusCode,
                Message = "Worker deleted successfully.",
                Data = null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for DeleteWorker: {ex}");
            throw;
        }
    }

    //Helpers
    private static string BuildQueryString(string basePath, WorkerFilterOptions options)
    {
        var queryParams = new List<string>();

        void AddIfNotNullOrEmpty(string key, object? value)
        {
            switch (value)
            {
                case int intValue:
                    queryParams.Add($"{key}={intValue}");
                    break;
                case string strValue when !string.IsNullOrWhiteSpace(strValue):
                    queryParams.Add($"{key}={strValue}");
                    break;
            }
        }

        AddIfNotNullOrEmpty("WorkerId", options.WorkerId);
        AddIfNotNullOrEmpty("Name", options.Name);
        AddIfNotNullOrEmpty("PhoneNumber", options.PhoneNumber);
        AddIfNotNullOrEmpty("Email", options.Email);
        AddIfNotNullOrEmpty("search", options.Search);
        AddIfNotNullOrEmpty("sortBy", options.SortBy);
        AddIfNotNullOrEmpty("sortOrder", options.SortOrder);

        return queryParams.Count > 0 ? $"{basePath}?{string.Join("&", queryParams)}" : basePath;
    }
}