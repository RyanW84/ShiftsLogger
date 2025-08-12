using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using Spectre.Console;

namespace ConsoleFrontEnd.Services;

public class LocationService : ILocationService
{
    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:7009/"),
    };

    public async Task<ApiResponseDto<List<Location>>> GetAllLocations(
        LocationFilterOptions locationFilterOptions
    )
    {
        try
        {
            var queryString = BuildQueryString("api/locations", locationFilterOptions);

            AnsiConsole.MarkupLine(
                $"[blue]Final request URL: {httpClient.BaseAddress}{queryString}[/]\n"
            );

            var response = await httpClient.GetAsync(queryString);
            if (!response.IsSuccessStatusCode)
            {
                AnsiConsole.Markup("[red]Locations not retrieved.[/]\n");
                return new ApiResponseDto<List<Location>>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase ?? "Unknown error",
                    Data = null,
                };
            }

            AnsiConsole.Markup("[green]Locations retrieved successfully.[/]\n");
            return await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Location>>>()
                ?? new ApiResponseDto<List<Location>>
                {
                    ResponseCode = response.StatusCode,
                    Message = "Data obtained",
                    Data = new List<Location>(),
                };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for GetAllLocations: {ex}");
            throw;
        }
    }

	public async Task<ApiResponseDto<Location>> GetLocationById(int id)
	{
		HttpResponseMessage response;
		try
		{
			response = await httpClient.GetAsync($"api/locations/{id}");

			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				return await response.Content.ReadFromJsonAsync<ApiResponseDto<Location>>()
					?? new ApiResponseDto<Location>
					{
						ResponseCode = response.StatusCode ,
						Message = "No data returned." ,
						Data = response.Content.ReadFromJsonAsync<Location>().Result ,
						TotalCount = 0 ,
					};
			}
			else
			{
				return await response.Content.ReadFromJsonAsync<ApiResponseDto<Location>>()
					?? new ApiResponseDto<Location>
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
			Console.WriteLine($"Try catch failed for GetLocationById: {ex}");
			throw;
		}
	}

	public async Task<ApiResponseDto<Location>> CreateLocation(Location createdLocation)
	{
		try
		{
			var response = await httpClient.PostAsJsonAsync("api/locations" , createdLocation);
			if (response.StatusCode != System.Net.HttpStatusCode.Created)
			{
				Console.WriteLine($"Error: Status Code - {response.StatusCode}");
				return new ApiResponseDto<Location>
				{
					ResponseCode = response.StatusCode ,
					Message = response.ReasonPhrase ?? "Creation failed" ,
					Data = null ,
				};
			}

			Console.WriteLine("Location created successfully.");
			return new ApiResponseDto<Location>
			{
				ResponseCode = response.StatusCode ,
				Data = await response.Content.ReadFromJsonAsync<Location>() ?? createdLocation ,
			};
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Try catch failed for CreateLocation: {ex}");
			throw;
		}
	}

	public async Task<ApiResponseDto<Location>> UpdateLocation(int id , Location updatedLocation)
	{
		HttpResponseMessage response;
		try
		{
			response = await httpClient.PutAsJsonAsync($"api/locations/{id}" , updatedLocation);
			if (response.StatusCode is not System.Net.HttpStatusCode.OK)
			{
				return new ApiResponseDto<Location>
				{
					ResponseCode = response.StatusCode ,
					Message = response.ReasonPhrase ,
					Data = null ,
				};
			}
			else
			{
				return await response.Content.ReadFromJsonAsync<ApiResponseDto<Location>>()
					?? new ApiResponseDto<Location>
					{
						ResponseCode = response.StatusCode ,
						Message = "Update Location succeeded." ,
						Data = null ,
					};
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Try catch failed for UpdateLocation: {ex}");
			throw;
		}
	}

	public async Task<ApiResponseDto<string>> DeleteLocation(int id)
	{
		try
		{
			var response = await httpClient.DeleteAsync($"api/locations/{id}");
			if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
			{
				return new ApiResponseDto<string>
				{
					ResponseCode = response.StatusCode ,
					Message = $"Error deleting Location - {response.StatusCode}" ,
					Data = null ,
				};
			}

			return new ApiResponseDto<string>
			{
				ResponseCode = response.StatusCode ,
				Message = "Location deleted successfully." ,
				Data = null ,
			};
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Try catch failed for DeleteLocation: {ex}");
			throw;
		}
	}

	private static string BuildQueryString(string basePath, LocationFilterOptions options)
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

        AddIfNotNullOrEmpty("locationId", options.LocationId);
        AddIfNotNullOrEmpty("name", options.Name);
        AddIfNotNullOrEmpty("address", options.Address);
        AddIfNotNullOrEmpty("townOrCity", options.Town);
        AddIfNotNullOrEmpty("stateOrCounty", options.County);
        AddIfNotNullOrEmpty("zipOrPostCode", options.PostCode);
        AddIfNotNullOrEmpty("country", options.Country);
        AddIfNotNullOrEmpty("search", options.Search);
        AddIfNotNullOrEmpty("sortBy", options.SortBy);
        AddIfNotNullOrEmpty("sortOrder", options.SortOrder);

        return queryParams.Count > 0 ? $"{basePath}?{string.Join("&", queryParams)}" : basePath;
    }
}
