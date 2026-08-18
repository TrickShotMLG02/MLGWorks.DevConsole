namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Operations that can be requested by input sources and built-in commands.
    /// </summary>
    public interface IConsoleActions : IConsoleOutput
    {
        bool IsVisible { get; }
        bool IsInputFieldFocused { get; }

        void ToggleVisibility();
        void SubmitInput();
        void RequestAutoComplete();
        void HistoryPrevious();
        void HistoryNext();
        void ClearLogs();
    }
}
