using System.Collections.Generic;
using MLGWorks.DevConsole.Runtime.Commands;

namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Provides command discovery and registration without exposing the concrete registry implementation.
    /// </summary>
    public interface ICommandRegistry
    {
        IReadOnlyDictionary<string, CommandInfo> Commands { get; }
        IEnumerable<CommandInfo> CommandInfos { get; }

        void RegisterAll();
        bool UnregisterCommand(string name);
    }
}
