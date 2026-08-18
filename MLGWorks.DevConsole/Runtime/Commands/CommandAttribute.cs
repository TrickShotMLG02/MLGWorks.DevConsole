using System;

namespace MLGWorks.DevConsole.Runtime.Commands
{
    public enum CommandDangerLevel
    {
        None = 0,
        Warning = 1,
        Dangerous = 2
    }

    /// <summary>
    /// Marks a static method as a console command.
    /// Methods decorated with this attribute can be invoked via the developer console.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// The primary name of the command.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A brief description of the command's purpose.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Optional alternative names (aliases) for the command.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        /// Indicates whether the command should be highlighted as potentially harmful.
        /// </summary>
        public CommandDangerLevel DangerLevel { get; set; }

        /// <summary>
        /// Controls the initial catalog state for newly discovered commands.
        /// Existing catalog state is preserved during rediscovery.
        /// </summary>
        public bool EnabledByDefault { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
        /// </summary>
        /// <param name="name">Primary command name.</param>
        /// <param name="description">Optional command description.</param>
        /// <param name="aliases">Optional aliases for the command.</param>
        public CommandAttribute(string name, string description = "", params string[] aliases)
        {
            Name = name;
            Description = description;
            Aliases = aliases ?? Array.Empty<string>();
        }
    }
}
