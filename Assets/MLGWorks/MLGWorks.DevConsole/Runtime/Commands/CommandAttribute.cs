using System;

namespace MLGWorks.DevConsole.Runtime.Commands
{
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
