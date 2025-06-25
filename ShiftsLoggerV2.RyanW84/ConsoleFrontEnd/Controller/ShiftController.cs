using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.Dtos;
using ConsoleFrontEnd.Models.FilterOptions;
using ConsoleFrontEnd.Services;
using Spectre.Console;

namespace ConsoleFrontEnd.Controller
{
	public class ShiftController( )
	{
		private readonly MenuSystem.UserInterface userInterface = new();
		private readonly ShiftService shiftService = new ();
		private readonly WorkerController workerController = new ();

		private ShiftFilterOptions shiftFilterOptions = new()
		{
			ShiftId = null ,
			WorkerId = null ,
			StartTime = null ,
			EndTime = null ,
			StartDate = null ,
			EndDate = null ,
			LocationName = null ,
			Search = null ,
			SortBy = null ,
			SortOrder = null ,
		};

		// Helpers
		public async Task<ApiResponseDto<Shift>> CheckShiftExists(int shiftId)
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
							.AddChoices(new[] { "Try Again" , "Exit" })
					);
					if (exitSelection is "Exit")
					{
						return new ApiResponseDto<Shift>
						{
							RequestFailed = true ,
							ResponseCode = System.Net.HttpStatusCode.NotFound ,
							Message = "User exited the operation." ,
							Data = null ,
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
				return new ApiResponseDto<Shift>
				{
					RequestFailed = true ,
					ResponseCode = System.Net.HttpStatusCode.InternalServerError ,
					Message = $"Exception occurred: {ex.Message}" ,
					Data = null ,
				};
			}
		}
		public async Task<ApiResponseDto<int>> SelectShift(
	  ShiftFilterOptions? shiftFilterOptions = null
  )
		{
			// Use default filter if none provided
			shiftFilterOptions ??= new ShiftFilterOptions();

			// Fetch shifts
			var response = await shiftService.GetAllShifts(shiftFilterOptions);

			if (response.Data == null || response.Data.Count == 0)
			{
				AnsiConsole.MarkupLine("[red]No shifts found.[/]");
				return new ApiResponseDto<int>
				{
					RequestFailed = true ,
					ResponseCode = response.ResponseCode ,
					Message = "No shifts available." ,
					Data = 0 ,
				};
			}

			// Prepare choices for the menu
			var choices = response.Data.Select(s => new { s.ShiftId, Display = $"{s.StartTime} {s.Location.Name}" }).ToList();

			// Show menu and get selection
			var selectedDisplay = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[yellow]Select a shift[/]")
					.AddChoices(choices.Select(c => c.Display))
			);

			// Find the selected shift's ID
			var selected = choices.First(c => c.Display == selectedDisplay);

			return new ApiResponseDto<int>
			{
				RequestFailed = false ,
				ResponseCode = response.ResponseCode ,
				Message = "Shift selected." ,
				Data = selected.ShiftId ,
			};
		}

		// CRUD
		public async Task CreateShift( )
		{
			try
			{
				Console.Clear();
				AnsiConsole.Write(
					new Rule("[bold yellow]Create Shift[/]").RuleStyle("yellow").Centered()
				);
				var workerId = await workerController.SelectWorker();
				var shift = userInterface.CreateShiftUi(workerId.Data);
				var createdShift = await shiftService.CreateShift(shift);
				userInterface.ContinueAndClearScreen();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex.Message}");
				userInterface.ContinueAndClearScreen();
			}
		}

		public async Task GetAllShifts( )
		{
			try
			{
				Console.Clear();
				AnsiConsole.Write(
					new Rule("[bold yellow]View All Shifts[/]").RuleStyle("yellow").Centered()
				);

				var filterOptions = userInterface.FilterShiftsUi();

				shiftFilterOptions = filterOptions;
				var response = await shiftService.GetAllShifts(shiftFilterOptions);

				if (response.Data is null)
				{
					AnsiConsole.MarkupLine("[red]No shifts found.[/]");
					userInterface.ContinueAndClearScreen();
				}
				else
					userInterface.DisplayShiftsTable(response.Data);
				userInterface.ContinueAndClearScreen();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex.Message}");
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
				ApiResponseDto<int>? shiftId = await SelectShift();
				ApiResponseDto<Shift> shift = await shiftService.GetShiftById(shiftId.Data);

				if (shift.Data is not null)
				{
					userInterface.DisplayShiftsTable([shift.Data]);
					userInterface.ContinueAndClearScreen();
				}
				else
				{
					userInterface.DisplayErrorMessage(shift.Message);
					userInterface.ContinueAndClearScreen();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex}");
			}
		}

		public async Task UpdateShift( )
		{
			try
			{
				Console.Clear();
				AnsiConsole.Write(
					new Rule("[bold yellow]Update Shift[/]").RuleStyle("yellow").Centered()
				);

				ApiResponseDto<int>? shiftId = await SelectShift();
				ApiResponseDto<Shift> existingShift = await shiftService.GetShiftById(
					shiftId.Data
				);

				var updatedShift = userInterface.UpdateShiftUi(existingShift.Data);

				var updatedShiftResponse = await shiftService.UpdateShift(
					shiftId.Data ,
					updatedShift
				);
				userInterface.DisplaySuccessMessage($"\n{updatedShiftResponse.Message}");
				userInterface.ContinueAndClearScreen();
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Exception: {ex.Message}[/]");
			}
		}

		public async Task DeleteShift( )
		{
			try
			{
				Console.Clear();
				AnsiConsole.Write(
					new Rule("[bold yellow]Delete Shift[/]").RuleStyle("yellow").Centered()
				);

				ApiResponseDto<int>? shiftId = await SelectShift();
				ApiResponseDto<Shift> existingShift = await shiftService.GetShiftById(
					shiftId.Data
				);

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
				Console.WriteLine($"Try Pass failed in Shift Controller: Delete Shift {ex}");
				userInterface.ContinueAndClearScreen();
			}
		}

		public async Task<bool> IsWorkerAvailableForShift(int workerId, DateTime newShiftStart, DateTime newShiftEnd)
		{
			// Fetch all shifts for the worker
			var filterOptions = new ShiftFilterOptions
			{
				WorkerId = workerId
			};
			var response = await GetAllShifts(filterOptions);

			if (response.Data == null)
				return true; // No shifts, so available

			// Check for overlap
			foreach (var shift in response.Data)
			{
				if (shift.StartTime < newShiftEnd && newShiftStart < shift.EndTime)
				{
					// Overlap detected
					return false;
				}
			}

			return true; // No overlap, worker is available
		}
	}
}
