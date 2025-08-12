using System.Net.Http.Json;
using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using Spectre.Console;

namespace ConsoleFrontEnd.Services;

public class ShiftService : IShiftService
{
    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:7009/"),
    };

    public async Task<ApiResponseDto<List<Shift>>> GetAllShifts(
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
                ApiResponseDto<List<Shift>> shiftsResponse =
                    await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Shift>>>()
                    ?? new ApiResponseDto<List<Shift>>
                    {
                        ResponseCode = response.StatusCode,
                        Message = "Data obtained",
                        Data = new List<Shift>(), // Fixed initialization of 'Data'
                    };

                return shiftsResponse;
            }
            else
            {
                var shiftsResponse =
                    await response.Content.ReadFromJsonAsync<ApiResponseDto<List<Shift>>>()
                    ?? new ApiResponseDto<List<Shift>>()
                    {
                        RequestFailed = true,
                        Message = $"{response.ReasonPhrase}",
                        Data = new List<Shift>(), // Fixed initialization of 'Data'
                    };
                return new ApiResponseDto<List<Shift>>
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

    public async Task<ApiResponseDto<Shift>> GetShiftById(int id)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"api/shifts/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponseDto<Shift>>()
                    ?? new ApiResponseDto<Shift>
                    {
                        ResponseCode = response.StatusCode,
                        Message = "No data returned.",
                        Data = response.Content.ReadFromJsonAsync<Shift>().Result,
                        TotalCount = 0,
                    };
            }
            else
            {
                return await response.Content.ReadFromJsonAsync<ApiResponseDto<Shift>>()
                    ?? new ApiResponseDto<Shift>
                    {
                        ResponseCode = response.StatusCode,
                        Message = "No data returned.",
                        Data = null,
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

    public async Task<ApiResponseDto<Shift>> CreateShift(Shift createdShift)
    {
        try
        {
            // Convert Shift to ShiftApiRequestDto
            var shiftDto = new ShiftApiRequestDto
            {
                WorkerId = createdShift.WorkerId,
                LocationId = createdShift.LocationId,
                StartTime = createdShift.StartTime,
                EndTime = createdShift.EndTime
            };
            
            var response = await httpClient.PostAsJsonAsync("api/shifts", shiftDto);
            
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                Console.WriteLine($"Error: Status Code - {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Content: {errorContent}");
                return new ApiResponseDto<Shift>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase ?? "Creation failed",
                    Data = null,
                    RequestFailed = true
                };
            }

            Console.WriteLine("Shift created successfully.");
            var responseData = await response.Content.ReadFromJsonAsync<ApiResponseDto<Shift>>();
            return responseData ?? new ApiResponseDto<Shift>
            {
                ResponseCode = response.StatusCode,
                Data = createdShift,
                RequestFailed = false
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for CreateShift: {ex}");
            throw;
        }
    }

    public async Task<ApiResponseDto<Shift>> UpdateShift(int id, Shift updatedShift)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync($"api/shifts/{id}", updatedShift);
            if (response.StatusCode is not System.Net.HttpStatusCode.OK)
            {
                return new ApiResponseDto<Shift>
                {
                    ResponseCode = response.StatusCode,
                    Message = response.ReasonPhrase,
                    Data = null,
                };
            }
            else
            {
                return await response.Content.ReadFromJsonAsync<ApiResponseDto<Shift>>()
                    ?? new ApiResponseDto<Shift>
                    {
                        ResponseCode = response.StatusCode,
                        Message = "Update Shift succeeded.",
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
        try
        {
            var response = await httpClient.DeleteAsync($"api/shifts/{id}");
            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                return new ApiResponseDto<string>
                {
                    ResponseCode = response.StatusCode,
                    Message = $"Error deleting Shift - {response.StatusCode}",
                    Data = null,
                };
            }

            return new ApiResponseDto<string>
            {
                ResponseCode = response.StatusCode,
                Message = "Shift deleted successfully.",
                Data = null,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Try catch failed for DeleteShift: {ex}");
            throw;
        }
    }

    //Helpers
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
        AddIfNotNullOrEmpty("StartTime", options.StartTime);
        AddIfNotNullOrEmpty("EndTime", options.EndTime);
        AddIfNotNullOrEmpty("LocationName", options.LocationName);
        AddIfNotNullOrEmpty("Search", options.Search);
        AddIfNotNullOrEmpty("SortBy", options.SortBy);
        AddIfNotNullOrEmpty("SortOrder", options.SortOrder);

        return queryParams.Count > 0 ? $"{basePath}?{string.Join("&", queryParams)}" : basePath;
    }
}
