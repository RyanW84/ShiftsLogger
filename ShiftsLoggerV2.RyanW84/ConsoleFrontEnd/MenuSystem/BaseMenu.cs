using Spectre.Console;

namespace ConsoleFrontEnd.MenuSystem;

public abstract class BaseMenu
{
    private static readonly object _consoleLock = new();

    protected static void DisplayHeader(string title, string color = "yellow")
    {
        lock (_consoleLock)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold]{title}[/]").RuleStyle(color).Centered());
            AnsiConsole.WriteLine();
        }
    }

    protected static void DisplayErrorMessage(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(message)}[/]");
        }
    }

    protected static void DisplaySuccessMessage(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[green]{Markup.Escape(message)}[/]");
        }
    }

    protected static void DisplayInfoMessage(string message)
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[blue]{Markup.Escape(message)}[/]");
        }
    }

    protected static void PauseForUserInput(string message = "Press any key to continue...")
    {
        lock (_consoleLock)
        {
            AnsiConsole.MarkupLine($"[dim]{Markup.Escape(message)}[/]");
            Console.ReadKey(true);
        }
    }

    protected static bool ConfirmAction(string action)
    {
        lock (_consoleLock)
        {
            return AnsiConsole.Confirm($"Are you sure you want to {action}?");
        }
    }

    protected static async Task ShowLoadingSpinnerAsync(string message, Func<Task> action)
    {
        try
        {
            await AnsiConsole
                .Status()
                .StartAsync(
                    message,
                    async context =>
                    {
                        context.Spinner(Spinner.Known.Star);
                        context.SpinnerStyle(Style.Parse("green"));
                        await action();
                    }
                );
        }
        catch (Exception)
        {
            // Ensure console is in a clean state after any exception
            AnsiConsole.Clear();
            throw;
        }
    }

    protected static void ClearConsoleState()
    {
        lock (_consoleLock)
        {
            AnsiConsole.Clear();
        }
    }
}
