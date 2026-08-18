using MLGWorks.DevConsole.Runtime.Commands;

namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Provides command suggestions and applies requested completions.
    /// </summary>
    public interface IAutocompleteEngine
    {
        string GetSuggestion(string input, out CommandInfo matchedCommand);
        void RequestAutoComplete();
        string TryPerformAutoComplete(string currentInput);
        void SetMatchedCommand(CommandInfo matchedCommand);
    }
}
