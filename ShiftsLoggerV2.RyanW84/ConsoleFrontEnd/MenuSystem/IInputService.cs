namespace ConsoleFrontEnd.MenuSystem;

public interface IInputService
{
    T GetInput<T>(string prompt, T? defaultValue = default);
    T GetValidatedInput<T>(string prompt, Func<T, bool> validator, string errorMessage, T? defaultValue = default);
    string GetSelection(string title, IEnumerable<string> choices);
    DateTime GetDateTime(string prompt, DateTime? minValue = null);
}
