using System.Collections.Generic;
using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Configuration;

namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Compatibility adapter exposing the existing static command manager as an interface.
    /// </summary>
    public sealed class CommandManagerRegistry : ICommandRegistry
    {
        public IReadOnlyDictionary<string, CommandInfo> Commands => CommandManager.Commands;
        public IEnumerable<CommandInfo> CommandInfos => CommandManager.CommandInfos;

        public void RegisterAll() => CommandManager.RegisterAll();

        public void RegisterFromSettings(DevConsoleCommandSettings settings) =>
            CommandManager.RegisterFromSettings(settings);
        public bool UnregisterCommand(string name) => CommandManager.UnregisterCommand(name);
    }
}
