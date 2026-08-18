using MLGWorks.Utils.Logging;

namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Receives console output independently of the Unity UI implementation.
    /// </summary>
    public interface IConsoleOutput
    {
        void AppendToOutput(string message, LogLevel? level = null);
    }
}
