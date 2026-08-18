namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Executes a command input string and returns its result.
    /// </summary>
    public interface ICommandExecutor
    {
        bool TryExecute(string input, out string result);
    }
}
