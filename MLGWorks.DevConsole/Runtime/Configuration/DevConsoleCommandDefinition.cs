using System;
using MLGWorks.DevConsole.Runtime.Commands;

namespace MLGWorks.DevConsole.Runtime.Configuration
{
    /// <summary>
    /// Editor-generated metadata for a command that can be exposed by the console.
    /// </summary>
    [Serializable]
    public sealed class DevConsoleCommandDefinition
    {
        public string commandName;
        public string description;
        public string[] aliases = Array.Empty<string>();
        public string assemblyName;
        public string declaringTypeName;
        public string methodName;
        public string[] parameterTypeNames = Array.Empty<string>();
        public bool enabled = true;
        public bool isObsolete;
        public bool isTestOnly;
        public CommandDangerLevel dangerLevel;

        public string StableId =>
            $"{assemblyName}|{declaringTypeName}|{methodName}|{string.Join(",", parameterTypeNames ?? Array.Empty<string>())}";
    }
}
