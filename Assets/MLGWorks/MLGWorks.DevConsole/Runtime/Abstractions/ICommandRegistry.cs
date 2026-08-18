using System.Collections.Generic;
using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Configuration;

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
        void RegisterFromSettings(DevConsoleCommandSettings settings);
        bool UnregisterCommand(string name);
    }
}
