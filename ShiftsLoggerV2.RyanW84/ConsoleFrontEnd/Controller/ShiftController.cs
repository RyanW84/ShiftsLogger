using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Spectre.Console;

namespace ConsoleFrontEnd.Controller
{
    public class ShiftController()
    {
        private readonly MenuSystem.UserInterface userInterface = new();
        private readonly ShiftService shiftService = new ShiftService();

        private ShiftFilterOptions shiftFilterOptions = new()
        {
            ShiftId = null,
            WorkerId = null,
            StartTime = null,
            EndTime = null,
            StartDate = null,
            EndDate = null,
            LocationName = null,
            Search = null,
            SortBy = null,
            SortOrder = null,
        };

        // Helpers
        public async Task<ApiResponseDto<Shifts>> CheckShiftExists(int shiftId)
        {
            try
            {
                var response = await shiftService.GetShiftById(shiftId);

                while (response.ResponseCode is not System.Net.HttpStatusCode.OK)
                {
                    userInterface.DisplayErrorMessage(response.Message);
                    Console.WriteLine();
                    var exitSelection = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Try again or exit?")
                            .AddChoices(new[] { "Try Again", "Exit" })
                    );
                    if (exitSelection is "Exit")
                    {
                        return new ApiResponseDto<Shifts>
                        {
                            RequestFailed = true,
                            ResponseCode = System.Net.HttpStatusCode.NotFound,
                            Message = "User exited the operation.",
                            Data = null,
                        };
                    }
                    else if (exitSelection is "Try Again")
                    {
                        AnsiConsole.Markup("[Yellow]Please enter a correct ID: [/]");
                        shiftId = userInterface.GetShiftByIdUi();
                        response = await shiftService.GetShiftById(shiftId);
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Try catch failed for CheckShiftExists: {ex}");
                return new ApiResponseDto<Shifts>
                {
                    RequestFailed = true,
                    ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"Exception occurred: {ex.Message}",
                    Data = null,
                };
            }
        }

        // CRUD
        public async Task CreateShift()
        {
            try
            {
                var shift = userInterface.CreateShiftUi();
                var createdShift = await shiftService.CreateShift(shift);
                if (createdShift.ResponseCode is System.Net.HttpStatusCode.Created)
                {
                    userInterface.DisplaySuccessMessage(createdShift.Message);
                    userInterface.ContinueAndClearScreen();
                }
                else
                {
                    userInterface.DisplayErrorMessage(createdShift.Message);
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
                ApiResponseDto<List<Shifts>> shifts = await shiftService.GetAllShifts(
                    shiftFilterOptions
                );

                if (shifts.ResponseCode is System.Net.HttpStatusCode.OK)
                {
                    userInterface.DisplaySuccessMessage(shifts.Message);
                    userInterface.DisplayShiftsTable(shifts.Data);
                    userInterface.ContinueAndClearScreen();
                }
                else
                {
                    userInterface.DisplayErrorMessage(shifts.Message);
                    userInterface.ContinueAndClearScreen();
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Exception: {ex.Message}[/]");
                userInterface.ContinueAndClearScreen();
            }
        }

		public async Task GetShiftById( )
		{
			try
			{
				Console.Clear();

				AnsiConsole.Write(
					new Rule("[bold yellow]View Shift by ID[/]").RuleStyle("yellow").Centered()
				);
				var shiftId = userInterface.GetShiftByIdUi();
				var shift = await CheckShiftExists(shiftId);

				if (shift.Data is not null)
				{
					userInterface.DisplayShiftsTable([shift.Data]);
				}
				else
					userInterface.DisplayErrorMessage(shift.Message);
				userInterface.ContinueAndClearScreen();
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
                Console.Clear();
                AnsiConsole.Write(
                    new Rule("[bold yellow]Update Shift[/]").RuleStyle("yellow").Centered()
                );

                var shiftId = userInterface.GetShiftByIdUi();

                var existingShift = await CheckShiftExists(shiftId);

                if (existingShift.Data is null)
                {
                    userInterface.DisplayErrorMessage(existingShift.Message);
                    userInterface.ContinueAndClearScreen();
                    return;
                }

                var updatedShift = userInterface.UpdateShiftUi(existingShift.Data);

                var updatedShiftResponse = await shiftService.UpdateShift(shiftId, updatedShift);
                userInterface.DisplaySuccessMessage($"\n{updatedShiftResponse.Message}");
                userInterface.ContinueAndClearScreen();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Exception: {ex.Message}[/]");
                userInterface.ContinueAndClearScreen();
            }
        }

        public async Task DeleteShift()
        {
            try
            {
                Console.Clear();
                AnsiConsole.Write(
                    new Rule("[bold yellow]Delete Shift[/]").RuleStyle("yellow").Centered()
                );

                var shiftId = userInterface.GetShiftByIdUi();
                var existingShift = await CheckShiftExists(shiftId);

                if (existingShift.Data is null)
                {
                    userInterface.DisplayErrorMessage(existingShift.Message);
                    userInterface.ContinueAndClearScreen();
                    return;
                }

                var deletedShiftResponse = await shiftService.DeleteShift(
                    existingShift.Data.ShiftId
                );

                if (deletedShiftResponse.RequestFailed)
                {
                    userInterface.DisplayErrorMessage(deletedShiftResponse.Message);
                }
                else
                {
                    userInterface.DisplaySuccessMessage($"\n{deletedShiftResponse.Message}");
                }

                userInterface.ContinueAndClearScreen();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Exception: {ex.Message}[/]");
                userInterface.ContinueAndClearScreen();
            }
        }
    }
}
