using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Spectre.Console;

namespace ConsoleFrontEnd.Controller
{
    public class ShiftController()
    {
        // This class acts as a controller for managing shifts, handling user input and interaction with the ShiftService.
        // It provides methods to create a shift and retrieve all shifts with optional filtering.
        private readonly MenuSystem.UserInterface userInterface = new();
        private readonly ShiftService shiftService = new ShiftService();

        private ShiftFilterOptions shiftFilterOptions = new()
        {
            ShiftId = null,
            WorkerId = null,
            LocationId = null,
            StartTime = null,
            EndTime = null,
            StartDate = null,
            EndDate = null,
            LocationName = null,
            SortBy = null,
            SortOrder = null,
            Search = null,
        };

        // Helpers
        public async Task<ApiResponseDto<Shifts>> ShiftsNotFoundHelper(int shiftId)
        {
            ApiResponseDto<Shifts>? checkedShift = await shiftService.GetShiftById(shiftId);
            while (checkedShift.ResponseCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine();
                var exitSelection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Try again or exit?")
                        .AddChoices(new[] { "Try Again", "Exit" })
                );
                if (exitSelection is "Exit")
                {
                    Console.Clear();
                    return new ApiResponseDto<Shifts>
                    {
                        RequestFailed = true,
                        ResponseCode = System.Net.HttpStatusCode.NotFound,
                        Message = "Shift not found.",
                        Data = null,
                    };
                }
                else
                {
                    Console.Clear();
                    shiftId = userInterface.GetShiftByIdUi();
                    checkedShift = await shiftService.GetShiftById(shiftId);
                }
            }

            return checkedShift;
        }

        // CRUD
        public async Task CreateShift()
        {
            try
            {
                var shift = userInterface.CreateShiftUi();
                var createdShift = await shiftService.CreateShift(shift);
                if (
                    createdShift.ResponseCode is not System.Net.HttpStatusCode.Created
                    || createdShift.ResponseCode is not System.Net.HttpStatusCode.OK
                )
                {
                    AnsiConsole.MarkupLine(
                        $"[red]Error: Failed to create shift: {createdShift.ResponseCode}[/]"
                    );
                }
                else
                {
                    AnsiConsole.MarkupLine("[green]Shift created successfully![/]");
                    AnsiConsole.MarkupLine($"[green]Shift ID: {createdShift.Data.ShiftId}[/]");
                    userInterface.ContinueAndClearScreen();
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Exception: {ex.Message}[/]");
                userInterface.ContinueAndClearScreen();
            }
        }

        public async Task GetAllShifts()
        {
            try
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(
                    new Rule("[bold yellow]View All Shifts[/]").RuleStyle("yellow").Centered()
                );

                var filterOptions = userInterface.FilterShiftsUi();

                shiftFilterOptions = filterOptions;
                ApiResponseDto<List<Shifts?>> shifts = await shiftService.GetAllShifts(shiftFilterOptions);

                userInterface.DisplayShiftsTable(shifts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

		public async Task GetShiftById( )
		{
			try
			{
				AnsiConsole.Clear();
				AnsiConsole.Write(
					new Rule("[bold yellow]View Shift by ID[/]").RuleStyle("yellow").Centered()
				);
				var shiftId = userInterface.GetShiftByIdUi();
				ApiResponseDto<Shifts?> shift = await ShiftsNotFoundHelper(shiftId);
				if (shift.ResponseCode is System.Net.HttpStatusCode.NotFound || shift.Data == null)
				{
					return;
				}

				// Fix for CS9174: Create a new ApiResponseDto<List<Shifts?>> object instead of using a collection initializer.
				var shiftsResponse = new ApiResponseDto<List<Shifts?>>()
				{
					RequestFailed = false ,
					ResponseCode = System.Net.HttpStatusCode.OK ,
					Message = "Shift retrieved successfully." ,
					Data = new List<Shifts?> { shift.Data }
				};

				userInterface.DisplayShiftsTable(shiftsResponse);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
			}
		}

        public async Task UpdateShift()
        {
            try
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(
                    new Rule("[bold yellow]Update Shift[/]").RuleStyle("yellow").Centered()
                );
                var shiftId = userInterface.GetShiftByIdUi();
                ApiResponseDto<Shifts> existingShift = await ShiftsNotFoundHelper(shiftId);

                if (existingShift.ResponseCode is System.Net.HttpStatusCode.NotFound)
                {
                    return;
                }

                var updatedShift = userInterface.UpdateShiftUi(existingShift.Data);
                var updatedShiftResponse = await shiftService.UpdateShift(shiftId, updatedShift);
                userInterface.ContinueAndClearScreen();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception - {ex.Message}");
            }
        }

        public async Task DeleteShift()
        {
            try
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(
                    new Rule("[bold yellow]Delete Shift[/]").RuleStyle("yellow").Centered()
                );
                var shiftId = userInterface.GetShiftByIdUi();
                var deletedShift = await ShiftsNotFoundHelper(shiftId);

                if (deletedShift.ResponseCode is System.Net.HttpStatusCode.NotFound)
                {
                    return;
                }
                var deleteResponse = await shiftService.DeleteShift(shiftId);
                userInterface.ContinueAndClearScreen();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Try Pass failed in Shift Controller: Delete Shift {ex}");
                userInterface.ContinueAndClearScreen();
            }
        }
    }
}
