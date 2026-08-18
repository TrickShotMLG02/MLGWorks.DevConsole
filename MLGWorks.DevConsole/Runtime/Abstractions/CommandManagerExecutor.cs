namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Compatibility adapter exposing the existing static command manager as an executor.
    /// </summary>
    public sealed class CommandManagerExecutor : ICommandExecutor
    {
        private readonly IConsoleOutput _output;

        public CommandManagerExecutor(IConsoleOutput output = null)
        {
            _output = output;
        }

        public bool TryExecute(string input, out string result) =>
            Commands.CommandManager.TryExecute(input, _output, out result);
    }
}
