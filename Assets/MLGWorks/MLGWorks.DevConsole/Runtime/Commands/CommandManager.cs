using MLGWorks.DevConsole.Runtime.UI;
using MLGWorks.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Logger = MLGWorks.Utils.Logging.Logger;

namespace MLGWorks.DevConsole.Runtime.Commands
{
    public static class CommandManager
    {
        private static readonly Dictionary<string, CommandInfo> _commands = new();

        public static IReadOnlyDictionary<string, CommandInfo> Commands => _commands;

        private static readonly HashSet<CommandInfo> _commandInfos = new();

        public static IEnumerable<CommandInfo> CommandInfos => _commandInfos.OrderBy(cmd => cmd.Name);

        public static void RegisterAll()
        {
            _commands.Clear();
            _commandInfos.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    foreach (var method in methods)
                    {
                        var attr = method.GetCustomAttribute<CommandAttribute>();
                        if (attr == null) continue;

                        var command = new CommandInfo(attr.Name, attr.Description, method, attr.Aliases);

                        // Register primary name
                        RegisterCommand(attr.Name, command);
                        _commandInfos.Add(command);

                        // Register aliases
                        foreach (var alias in attr.Aliases)
                        {
                            RegisterCommand(alias, command);
                        }
                    }
                }
            }
        }

        private static void RegisterCommand(string name, CommandInfo command)
        {
            var key = name.ToLowerInvariant();
            if (_commands.ContainsKey(key))
            {
                Logger.Warning($"[DevConsole] Duplicate command or alias '{name}' ignored.");
                return;
            }

            _commands[key] = command;
        }

        public static bool TryExecute(string input, out string result)
        {
            result = string.Empty;
            if (string.IsNullOrWhiteSpace(input)) return false;

            // print entered command to terminal
            ConsoleUI.Instance.AppendToOutput($"> {input}", LogLevel.Command);

            var parts = input.Split(' ');
            var commandName = parts[0].ToLower();
            var args = parts.Skip(1).ToArray();

            if (!_commands.TryGetValue(commandName, out var command))
            {
                result = $"Unknown command: {commandName}";
                return false;
            }

            try
            {
                var parameters = command.Method.GetParameters();
                var parsedArgs = new object[parameters.Length];

                if (args.Length < parameters.Count(p => !p.IsOptional))
                {
                    result = command.GetUsage(commandName);
                    return false;
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];

                    // Handle params (variable-length) parameter
                    bool isParams = Attribute.IsDefined(param, typeof(ParamArrayAttribute));
                    if (isParams)
                    {
                        var elementType = param.ParameterType.GetElementType();
                        int paramsCount = args.Length - i;
                        Array paramsArray = Array.CreateInstance(elementType, paramsCount);

                        for (int j = 0; j < paramsCount; j++)
                        {
                            paramsArray.SetValue(Convert.ChangeType(args[j], elementType), j);
                        }

                        parsedArgs[i] = paramsArray;
                        break; // No more parameters after params[]
                    }

                    if (i >= args.Length)
                        parsedArgs[i] = param.DefaultValue;
                    else
                        parsedArgs[i] = Convert.ChangeType(args[i], param.ParameterType);
                }

                var returnValue = command.Method.Invoke(null, parsedArgs);
                result = returnValue?.ToString() ?? null;
                return true;
            }
            catch (Exception ex)
            {
                result = $"Command error: {ex.InnerException?.Message ?? ex.Message}";
                return false;
            }
        }
    }

    public class CommandInfo
    {
        public string Name { get; }
        public string Description { get; }
        public MethodInfo Method { get; }
        public String[] Aliases { get; }

        public CommandInfo(string name, string description, MethodInfo method, string[] aliases = null)
        {
            Name = name;
            Description = description;
            Method = method;
            Aliases = aliases;
        }

        public string GetHelp()
        {
            string aliasText = Aliases.Length > 0
                ? $" (aliases: {string.Join(", ", Aliases)})"
                : string.Empty;

            return $"{Name}{aliasText}\n  {Description}\n  {GetUsage(Name)}".Trim();
        }

        public string GetUsage(string name)
        {
            string parameters = string.Join(" ", Method.GetParameters()
                .Select(p =>
                {
                    string param = $"{Core.Utils.GetReadableTypeName(p.ParameterType)} {p.Name}";
                    return p.IsOptional ? $"[{param}]" : $"<{param}>";
                })
            );

            return $"Usage: {Name} {parameters}".Trim();
        }
    }

    public enum CommandOutputType
    {
        Info,
        Error,
        Warning,
        Debug,
        CommandOutput
    }
}
