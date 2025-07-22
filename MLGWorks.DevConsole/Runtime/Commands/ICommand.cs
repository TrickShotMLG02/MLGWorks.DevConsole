namespace MLGWorks.DevConsole.Runtime.Commands
{
    /// <summary>
    /// Interface for console commands.
    /// </summary>
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }

        void Execute(string[] args);
    }
}
