using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace ConsoleFrontEnd.Services;

public class ShiftService : IShiftService
{
    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:7009/"),
    };

    public async Task<ApiResponseDto<List<Shifts>>> GetAllShifts(
        ShiftFilterOptions shiftFilterOptions
    )
    {
        try
        {
            var queryString = BuildQueryString("api/shifts", shiftFilterOptions);

            AnsiConsole.MarkupLine(
                $"[blue]Final request URL: {httpClient.BaseAddress}{queryString}[/]\n"
            );

            // Make the request
            var response = await httpClient.GetAsync(queryString);

            if (response.StatusCode is System.Net.HttpStatusCode.OK)
            {
                ApiResponseDto<List<Shifts>> shiftsResponse =
                    await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Shifts>>>()
                    ?? new ApiResponseDto<List<Shifts>>
                    {
                        ResponseCode = response.StatusCode,
                        Message = "Data obtained",
                        Data = new List<Shifts>(), // Fixed initialization of 'Data'
                    };

                return shiftsResponse;
            }
            else
            {
                var shiftsResponse =
                    await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Shifts>>>()
                    ?? new ApiResponseDto<List<Shifts>>()
                    {
                        RequestFailed = true,
                        Message = $"{response.ReasonPhrase}",
                        Data = new List<Shifts>(), // Fixed initialization of 'Data'
                    };
                return new ApiResponseDto<List<Shifts>>
                {
                    ResponseCode = response.StatusCode,
                    Message = shiftsResponse.Message,
                    Data = null,
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for GetAllShifts: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Shifts>> GetShiftById(int id)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"api/shifts/{id}");

            if (response.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                return new ApiResponseDto<Shifts>
                {
                    ResponseCode = response.StatusCode,
                    Message = $"Shift Error: {response.StatusCode}",
                    Data = null,
                };
            }
            else
            {
                return await response.Content.ReadFromJsonAsync<ApiResponseDto<Shifts>>()
                    ?? new ApiResponseDto<Shifts>
                    {
                        ResponseCode = response.StatusCode,
                        Message = "Shift obtained",
                        Data = response.Content.ReadFromJsonAsync<Shifts>().Result,
                        TotalCount = 0,
                    };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for GetShiftById: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Shifts>> CreateShift(Shifts createdShift)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("api/shifts", createdShift);
            if (response.StatusCode is not System.Net.HttpStatusCode.Created)
            {
                return new ApiResponseDto<Shifts>
                {
                    ResponseCode = response.StatusCode,
                    Data = createdShift,
                    Message = "Shift created successfully.",
                };
            }
            else
            {
                return new ApiResponseDto<Shifts>
                {
                    RequestFailed = true,
                    ResponseCode = response.StatusCode,
                    Message = $"Create Shift Error: {response.ReasonPhrase}",
                    Data = null,
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for CreateShift: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Shifts>> UpdateShift(int id, Shifts updatedShift)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync($"api/shifts/{id}", updatedShift);
            if (response.StatusCode is System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponseDto<Shifts>>()
                    ?? new ApiResponseDto<Shifts>
                    {
                        ResponseCode = response.StatusCode,
                        Message = "Update Shift succeeded.",
                        Data = updatedShift,
                    };
            }
            else
            {
                return await response.Content.ReadFromJsonAsync<ApiResponseDto<Shifts>>()
                    ?? new ApiResponseDto<Shifts>
                    {
                        RequestFailed = true,
                        ResponseCode = response.StatusCode,
                        Message = $"Update Shift failed: {response.StatusCode}",
                        Data = null,
                    };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for UpdateShift: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<string>> DeleteShift(int id)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.DeleteAsync($"api/shifts/{id}");
            if (response.StatusCode is System.Net.HttpStatusCode.NoContent)
            {
                return new ApiResponseDto<string>
                {
                    ResponseCode = response.StatusCode,
                    Message = "Delete Shift succeeded.",
                    Data = null,
                };
            }
            else
            {
                return new ApiResponseDto<string>
                {
                    ResponseCode = response.StatusCode,
                    Message = $"Delete Shift Error: {response.StatusCode}",
                    Data = null,
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for DeleteShift: {ex}");
            throw;
        }
    }

    private static string BuildQueryString(string basePath, ShiftFilterOptions options)
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

        AddIfNotNullOrEmpty("ShiftId", options.ShiftId);
        AddIfNotNullOrEmpty("WorkerId", options.WorkerId);
        AddIfNotNullOrEmpty("LocationId", options.LocationId);
        AddIfNotNullOrEmpty("LocationName", options.LocationName);
        AddIfNotNullOrEmpty("StartDate", options.StartDate);
        AddIfNotNullOrEmpty("StartTime", options.StartTime);
        AddIfNotNullOrEmpty("EndDate", options.EndDate);
        AddIfNotNullOrEmpty("EndTime", options.EndTime);
        AddIfNotNullOrEmpty("search", options.Search);
        AddIfNotNullOrEmpty("sortBy", options.SortBy);
        AddIfNotNullOrEmpty("sortOrder", options.SortOrder);

        return queryParams.Count > 0 ? $"{basePath}?{string.Join("&", queryParams)}" : basePath;
    }
}
