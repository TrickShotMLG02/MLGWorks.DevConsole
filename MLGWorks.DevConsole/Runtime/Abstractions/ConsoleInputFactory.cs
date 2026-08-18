namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Creates the Unity Input System-backed console input source.
    /// </summary>
    public sealed class ConsoleInputFactory : IConsoleInputFactory
    {
        public IConsoleInput Create() => new ConsoleInputSource();
    }
}
