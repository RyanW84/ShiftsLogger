namespace ConsoleFrontEnd.MenuSystem;

public interface IDisplayService
{
    void DisplaySuccessMessage(string message);
    void DisplayErrorMessage(string message);
    void ContinueAndClearScreen();
}
