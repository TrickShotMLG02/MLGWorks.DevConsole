using System;

namespace MLGWorks.DevConsole.Runtime.Commands
{
    /// <summary>
    /// Marks a static method as a console command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public string[] Aliases { get; }

        public CommandAttribute(string name, string description = "", params string[] aliases)
        {
            Name = name;
            Description = description;
            Aliases = aliases ?? Array.Empty<string>();
        }
    }
}
