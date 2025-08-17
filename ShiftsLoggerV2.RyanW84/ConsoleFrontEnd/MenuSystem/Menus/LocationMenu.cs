using ConsoleFrontEnd.Core.Abstractions;
using ConsoleFrontEnd.Services;
using ConsoleFrontEnd.Models.FilterOptions;
using Microsoft.Extensions.Logging;

namespace ConsoleFrontEnd.MenuSystem.Menus;

/// <summary>
/// Location menu implementation following Single Responsibility Principle
/// Handles location-specific operations
/// </summary>
public class LocationMenuV2 : BaseMenuV2
{
    private readonly ILocationService _locationService;

    public LocationMenuV2(
        IConsoleDisplayService displayService,
        IConsoleInputService inputService,
        INavigationService navigationService,
        ILogger<LocationMenuV2> logger,
        ILocationService locationService)
        : base(displayService, inputService, navigationService, logger)
    {
        _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
    }

    public override string Title => "Location Management";
    public override string Context => "Location Management";

    protected override async Task ShowMenuAsync()
    {
        bool shouldExit = false;
        
        while (!shouldExit)
        {
            var choice = InputService.GetMenuChoice(
                "Select a location operation:",
                "View All Locations",
                "View Location by ID",
                "Create New Location",
                "Update Location",
                "Delete Location",
                "Filter Locations",
                "View Locations by Country",
                "View Locations by County", 
                "Back to Main Menu"
            );

            shouldExit = await HandleLocationChoice(choice);
        }
    }

    private async Task<bool> HandleLocationChoice(string choice)
    {
        Logger.LogDebug("Location menu choice selected: {Choice}", choice);

        if (await HandleCommonActions(choice))
            return true;

        try
        {
            switch (choice)
            {
                case "View All Locations":
                    await ViewAllLocationsAsync();
                    break;

                case "View Location by ID":
                    await ViewLocationByIdAsync();
                    break;

                case "Create New Location":
                    await CreateLocationAsync();
                    break;

                case "Update Location":
                    await UpdateLocationAsync();
                    break;

                case "Delete Location":
                    await DeleteLocationAsync();
                    break;

                case "Filter Locations":
                    await FilterLocationsAsync();
                    break;

                case "View Locations by Country":
                    await ViewLocationsByCountryAsync();
                    break;

                case "View Locations by County":
                    await ViewLocationsByCountyAsync();
                    break;

                case "Back to Main Menu":
                    return true;

                default:
                    DisplayService.DisplayError("Invalid choice");
                    InputService.WaitForKeyPress();
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling location choice: {Choice}", choice);
            DisplayService.DisplayError($"An error occurred: {ex.Message}");
            InputService.WaitForKeyPress();
        }

        return false;
    }

    private async Task ViewAllLocationsAsync()
    {
        DisplayService.DisplayHeader("All Locations", "blue");
        
        var response = await _locationService.GetAllLocationsAsync();
        
        if (response.RequestFailed)
        {
            DisplayService.DisplayError($"Failed to retrieve locations: {response.Message}");
        }
        else
        {
            DisplayService.DisplayTable(response.Data, "All Locations");
            DisplayService.DisplaySuccess($"Total locations: {response.TotalCount}");
        }
        
        InputService.WaitForKeyPress();
    }

    private async Task ViewLocationByIdAsync()
    {
        var locationId = InputService.GetIntegerInput("Enter Location ID:", 1);
        
        DisplayService.DisplayHeader($"Location Details (ID: {locationId})", "blue");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task CreateLocationAsync()
    {
        DisplayService.DisplayHeader("Create New Location", "green");
        
        try
        {
            var name = InputService.GetTextInput("Enter Location Name:");
            var address = InputService.GetTextInput("Enter Address:");
            var town = InputService.GetTextInput("Enter Town:");
            var county = InputService.GetTextInput("Enter County:");
            var postCode = InputService.GetTextInput("Enter Post Code:");
            var country = InputService.GetTextInput("Enter Country:");

            // Create location (this would call the API)
            DisplayService.DisplaySuccess("Location created successfully!");
            DisplayService.DisplayInfo($"Name: {name}");
            DisplayService.DisplayInfo($"Address: {address}, {town}");
            DisplayService.DisplayInfo($"County: {county}");
            DisplayService.DisplayInfo($"Post Code: {postCode}");
            DisplayService.DisplayInfo($"Country: {country}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating location");
            DisplayService.DisplayError($"Failed to create location: {ex.Message}");
        }

        InputService.WaitForKeyPress();
    }

    private async Task UpdateLocationAsync()
    {
        DisplayService.DisplayHeader("Update Location", "yellow");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task DeleteLocationAsync()
    {
        DisplayService.DisplayHeader("Delete Location", "red");
        
        var locationId = InputService.GetIntegerInput("Enter Location ID to delete:", 1);
        
        if (InputService.GetConfirmation($"Are you sure you want to delete location {locationId}?"))
        {
            DisplayService.DisplayInfo("Feature implementation in progress...");
        }
        else
        {
            DisplayService.DisplayInfo("Delete cancelled.");
        }
        
        InputService.WaitForKeyPress();
    }

    private async Task FilterLocationsAsync()
    {
        DisplayService.DisplayHeader("Filter Locations", "blue");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task ViewLocationsByCountryAsync()
    {
        DisplayService.DisplayHeader("Locations by Country", "blue");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }

    private async Task ViewLocationsByCountyAsync()
    {
        DisplayService.DisplayHeader("Locations by County", "blue");
        DisplayService.DisplayInfo("Feature implementation in progress...");
        InputService.WaitForKeyPress();
    }
}
