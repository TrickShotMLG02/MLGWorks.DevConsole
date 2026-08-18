namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Creates input sources, allowing production and test implementations to be selected independently.
    /// </summary>
    public interface IConsoleInputFactory
    {
        IConsoleInput Create();
    }
}
