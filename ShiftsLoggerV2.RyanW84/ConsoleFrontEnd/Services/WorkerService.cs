using Azure;

using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;

using Spectre.Console;

using System.Net.Http.Json;

namespace ConsoleFrontEnd.Services;

public class WorkerService : IWorkerService
{
    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:7009/"),
    };

    public async Task<ApiResponseDto<List<Workers>>> GetAllWorkers(
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
                return new ApiResponseDto<List<Workers>>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase ?? "Unknown error",
                    Data = null,
                };
            }

            AnsiConsole.Markup("[green]Workers retrieved successfully.[/]\n");
            return await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Workers>>>()
                ?? new ApiResponseDto<List<Workers>>
                {
                    ResponseCode = response.StatusCode,
                    Message = "Data obtained",
                    Data = new List<Workers>(),
                };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for GetAllWorkers: {ex}");
            throw;
        }
    }

	public async Task<ApiResponseDto<Workers>> GetWorkerById(int id)
	{
		HttpResponseMessage response;
		try
		{
			response = await httpClient.GetAsync($"api/workers/{id}");

			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				return await response.Content.ReadFromJsonAsync<ApiResponseDto<Workers>>()
					?? new ApiResponseDto<Workers>
					{
						ResponseCode = response.StatusCode ,
						Message = "No data returned." ,
						Data = response.Content.ReadFromJsonAsync<Workers>().Result,
						TotalCount = 0 ,
					};
			}
			else
			{
				return await response.Content.ReadFromJsonAsync<ApiResponseDto<Workers>>()
					?? new ApiResponseDto<Workers>
					{
						ResponseCode = response.StatusCode ,
						Message = "No data returned." ,
						Data = null ,
						TotalCount = 0 ,
					};
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Try catch failed for GetWorkerById: {ex}");
			throw;
		}
	}

    public async Task<ApiResponseDto<Workers>> CreateWorker(Workers createdWorker)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/workers", createdWorker);
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                Console.WriteLine($"Error: Status Code - {response.StatusCode}");
                return new ApiResponseDto<Workers>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase ?? "Creation failed",
                    Data = null,
                };
            }

            Console.WriteLine("Worker created successfully.");
            return new ApiResponseDto<Workers>
            {
                ResponseCode = response.StatusCode,
                Data = await response.Content.ReadFromJsonAsync<Workers>() ?? createdWorker,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for CreateWorker: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Workers>> UpdateWorker(int id, Workers updatedWorker)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync($"api/workers/{id}", updatedWorker);
            if (response.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                return new ApiResponseDto<Workers>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase,
                    Data = null,
                };
            }
            else
            {
                return await response.Content.ReadFromJsonAsync<ApiResponseDto<Workers>>()
                    ?? new ApiResponseDto<Workers>
                    {
                        ResponseCode = response.StatusCode,
                        Message = "Update Worker succeeded.",
                        Data = null,
                    };
            }
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
            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                return new ApiResponseDto<string>
                {
                    ResponseCode = response.StatusCode,
                    Message = $"Error deleting Worker - {response.StatusCode}",
                    Data = null,
                };
            }

            return new ApiResponseDto<string>
            {
                ResponseCode = response.StatusCode,
                Message = "Worker deleted successfully.",
                Data = null,
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
