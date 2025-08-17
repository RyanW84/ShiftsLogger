using ConsoleFrontEnd.UserInterface;
using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public class MainMenu : BaseMenu
{
    private static bool _continueLoop = true;
    private static bool _firstRun = true;

    public static async Task DisplayMainMenu()
    {
        if (_firstRun)
        {
            MenuHelpers.ShowWelcomeScreen();
            _firstRun = false;
        }

        while (_continueLoop)
            try
            {
                // Ensure clean state before displaying menu
                ClearConsoleState();
                DisplayHeader("Main Menu", "blue");
                MenuHelpers.ShowBreadcrumb("Main Menu");

                var choice = MenuHelpers.GetMenuChoice(
                    "Select an option:",
                    "Shift Management",
                    "Location Management",
                    "Worker Management",
                    "System Information",
                    "Exit Application"
                );

                await HandleMenuChoice(choice);
            }
            catch (Exception ex)
            {
                // Ensure clean state before showing error
                ClearConsoleState();
                DisplayErrorMessage($"An unexpected error occurred: {ex.Message}");
                PauseForUserInput();
            }
    }

    private static async Task HandleMenuChoice(string choice)
    {
        switch (choice)
        {
            case "Shift Management":
                await ShiftMenu.DisplayShiftMenu();
                break;
            case "Location Management":
                await LocationMenu.DisplayLocationMenu();
                break;
            case "Worker Management":
                await WorkerMenu.DisplayWorkerMenu();
                break;
            case "System Information":
                ShowSystemInformation();
                break;
            case "Exit Application":
                await ExitApplication();
                break;
            default:
                DisplayErrorMessage("Invalid choice entered");
                PauseForUserInput();
                break;
        }
    }

    private static void ShowSystemInformation()
    {
        DisplayHeader("System Information", "cyan");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1);

        table.AddColumn("[bold]Property[/]");
        table.AddColumn("[bold]Value[/]");

        table.AddRow("Application", "Shifts Logger Console");
        table.AddRow("Version", "2.0");
        table.AddRow(".NET Version", ".NET 9");
        table.AddRow("Last Updated", DateTime.Now.ToString("yyyy-MM-dd"));
        table.AddRow("Features", "Shift, Worker, and Location Management");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        PauseForUserInput();
    }

    private static async Task ExitApplication()
    {
        if (ConfirmAction("exit the application"))
        {
            DisplayHeader("Goodbye!", "green");

            var farewell = new Panel(
                    new Markup("[green]Thank you for using Shifts Logger![/]\n[dim]Have a great day![/]"))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green);

            AnsiConsole.Write(farewell);

            // Add a small delay for better UX
            await Task.Delay(1500);
            _continueLoop = false;
        }
    }

    public static void ReturnToMainMenu()
    {
        // This method can be called from sub-menus to return to main menu
        // without recursion
        DisplayInfoMessage("Returning to Main Menu...");
        Task.Delay(500).Wait();
    }
}