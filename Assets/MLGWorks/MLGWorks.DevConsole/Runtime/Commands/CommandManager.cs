using MLGWorks.DevConsole.Runtime.UI;
using MLGWorks.DevConsole.Runtime.Utils;
using MLGWorks.DevConsole.Runtime.Abstractions;
using MLGWorks.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Logger = MLGWorks.Utils.Logging.Logger;

namespace MLGWorks.DevConsole.Runtime.Commands
{
    /// <summary>
    /// Manages registration, lookup, execution, and unregistration of developer console commands.
    /// </summary>
    public static class CommandManager
    {
        /// <summary>
        /// Interface view of the legacy static command registry.
        /// </summary>
        public static ICommandRegistry Registry { get; } = new CommandManagerRegistry();

        /// <summary>
        /// Optional output sink used by callers of the legacy static execution API.
        /// </summary>
        public static IConsoleOutput Output { get; set; }

        // Internal dictionary mapping command names and aliases (lowercase) to CommandInfo instances.
        private static readonly Dictionary<string, CommandInfo> _commands = new();

        /// <summary>
        /// Read-only dictionary of all registered commands keyed by their names and aliases in lowercase.
        /// </summary>
        public static IReadOnlyDictionary<string, CommandInfo> Commands => _commands;

        // HashSet holding unique CommandInfo objects for iteration and ordering.
        private static readonly HashSet<CommandInfo> _commandInfos = new();

        /// <summary>
        /// All registered commands sorted alphabetically by command name.
        /// </summary>
        public static IEnumerable<CommandInfo> CommandInfos => _commandInfos.OrderBy(cmd => cmd.Name);

        /// <summary>
        /// Scans all loaded assemblies, finds methods decorated with <see cref="CommandAttribute"/>,
        /// and registers commands and their aliases.
        /// </summary>
        public static void RegisterAll()
        {
            _commands.Clear();
            _commandInfos.Clear();

            var assemblies = ReflectionUtils.GetSortedAssemblies();

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

                        // Register primary command name
                        RegisterCommand(attr.Name, command);

                        if (!_commandInfos.Contains(command))
                            _commandInfos.Add(command);

                        // Register all aliases
                        foreach (var alias in attr.Aliases)
                        {
                            RegisterCommand(alias, command);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers a single command or alias with the internal dictionary.
        /// Ignores duplicates and logs a warning.
        /// </summary>
        /// <param name="name">Command or alias name (case-insensitive).</param>
        /// <param name="command">Associated <see cref="CommandInfo"/>.</param>
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

        /// <summary>
        /// Unregisters a command and all its aliases by name or alias.
        /// </summary>
        /// <param name="name">Command name or alias to unregister.</param>
        /// <returns>True if the command was found and unregistered; otherwise false.</returns>
        public static bool UnregisterCommand(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            string key = name.ToLowerInvariant();

            // Find the CommandInfo associated with this command name or alias
            if (!_commands.TryGetValue(key, out var command))
                return false;

            // Remove all dictionary entries pointing to this CommandInfo
            var keysToRemove = _commands
                .Where(kvp => kvp.Value == command)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var k in keysToRemove)
            {
                _commands.Remove(k);
            }

            // Remove the command info from the unique set
            _commandInfos.Remove(command);

            return true;
        }

        /// <summary>
        /// Attempts to execute a command given the full input string.
        /// Parses arguments, converts types, and invokes the command method.
        /// </summary>
        /// <param name="input">Full user input string.</param>
        /// <param name="result">Output result or error message.</param>
        /// <returns>True if the command executed successfully; false otherwise.</returns>
        public static bool TryExecute(string input, out string result)
        {
            return TryExecute(input, Output, out result);
        }

        /// <summary>
        /// Attempts to execute a command and optionally writes the command echo to an output sink.
        /// </summary>
        public static bool TryExecute(string input, IConsoleOutput output, out string result)
        {
            result = string.Empty;
            if (string.IsNullOrWhiteSpace(input)) return false;

            // Print entered command to the supplied output sink when one is available.
            output?.AppendToOutput($"> {input}", LogLevel.Command);

            var parts = ParseArguments(input);
            var commandName = parts[0].ToLowerInvariant();
            var args = parts.Skip(1).ToList();

            if (!_commands.TryGetValue(commandName, out var command))
            {
                result = $"Unknown command: {commandName}";
                return false;
            }

            try
            {
                var parameters = command.Method.GetParameters();
                var parsedArgs = new object[parameters.Length];

                int requiredParameterCount = parameters.Count(p =>
                    !p.IsOptional && !Attribute.IsDefined(p, typeof(ParamArrayAttribute)));

                if (args.Count < requiredParameterCount)
                {
                    result = command.GetUsage();
                    return false;
                }

                int argumentIndex = 0;
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];

                    // Handle params (variable-length) parameter
                    bool isParams = Attribute.IsDefined(param, typeof(ParamArrayAttribute));
                    if (isParams)
                    {
                        var elementType = param.ParameterType.GetElementType();
                        int paramsCount = args.Count - argumentIndex;
                        Array paramsArray = Array.CreateInstance(elementType, paramsCount);

                        for (int j = 0; j < paramsCount; j++)
                            paramsArray.SetValue(ConvertArgument(args[argumentIndex++], elementType), j);

                        parsedArgs[i] = paramsArray;
                        continue;
                    }

                    // Handle string[] and other arrays manually
                    if (param.ParameterType == typeof(string[]))
                    {
                        parsedArgs[i] = args.Skip(argumentIndex).ToArray();
                        argumentIndex = args.Count;
                        continue;
                    }
                    else if (param.ParameterType.IsArray)
                    {
                        var elementType = param.ParameterType.GetElementType();
                        int arrayLen = args.Count - argumentIndex;
                        var array = Array.CreateInstance(elementType, arrayLen);

                        for (int j = 0; j < arrayLen; j++)
                            array.SetValue(ConvertArgument(args[argumentIndex++], elementType), j);

                        parsedArgs[i] = array;
                        continue;
                    }
                    else if (argumentIndex >= args.Count)
                    {
                        parsedArgs[i] = param.DefaultValue;
                    }
                    else
                    {
                        parsedArgs[i] = ConvertArgument(args[argumentIndex++], param.ParameterType);
                    }
                }

                if (argumentIndex < args.Count)
                {
                    result = command.GetUsage();
                    return false;
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

        private static object ConvertArgument(string value, Type targetType)
        {
            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(bool))
                return ReflectionUtils.ParseValue(targetType, new[] { value });

            if (targetType.IsEnum)
                return Enum.Parse(targetType, value, true);

            return Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static List<string> ParseArguments(string input)
        {
            var arguments = new List<string>();
            var current = new StringBuilder();
            char quote = '\0';
            bool escaped = false;

            foreach (char character in input.Trim())
            {
                if (escaped)
                {
                    current.Append(character);
                    escaped = false;
                    continue;
                }

                if (character == '\\' && quote != '\'')
                {
                    escaped = true;
                    continue;
                }

                if (quote != '\0')
                {
                    if (character == quote)
                        quote = '\0';
                    else
                        current.Append(character);
                    continue;
                }

                if (character == '\'' || character == '"')
                {
                    quote = character;
                }
                else if (char.IsWhiteSpace(character))
                {
                    if (current.Length > 0)
                    {
                        arguments.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(character);
                }
            }

            if (escaped)
                current.Append('\\');
            if (current.Length > 0)
                arguments.Add(current.ToString());

            return arguments;
        }
    }

    /// <summary>
    /// Represents metadata and reflection info for a console command.
    /// </summary>
    public class CommandInfo
    {
        /// <summary>
        /// Command primary name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Command description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Underlying method implementing the command.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Command aliases (alternative names).
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        /// Creates a new <see cref="CommandInfo"/> instance.
        /// </summary>
        /// <param name="name">Primary command name.</param>
        /// <param name="description">Description of the command.</param>
        /// <param name="method">Method implementing the command.</param>
        /// <param name="aliases">Optional aliases for the command.</param>
        public CommandInfo(string name, string description, MethodInfo method, string[] aliases = null)
        {
            Name = name;
            Description = description;
            Method = method;
            Aliases = aliases ?? Array.Empty<string>();
        }

        /// <summary>
        /// Returns detailed help text including command name, aliases, description, and usage.
        /// </summary>
        /// <returns>Formatted help string.</returns>
        public string GetHelp()
        {
            string aliasText = Aliases.Length > 0
                ? $" (aliases: {string.Join(", ", Aliases)})"
                : string.Empty;

            return $"{Name}{aliasText}\n  {Description}\n  {GetUsage()}".Trim();
        }

        /// <summary>
        /// Returns the command signature string including parameter names and types.
        /// Optional parameters are wrapped in square brackets.
        /// </summary>
        /// <returns>Command scheme string.</returns>
        public string GetCommandScheme()
        {
            string parameters = string.Join(" ", Method.GetParameters()
                .Select(p =>
                {
                    string param = $"{Core.Utils.GetReadableTypeName(p.ParameterType)} {p.Name}";
                    return p.IsOptional ? $"[{param}]" : $"<{param}>";
                })
            );
            return $"{Name} {parameters}".Trim();
        }

        /// <summary>
        /// Returns a usage string that explains how to call the command.
        /// </summary>
        /// <returns>Usage string.</returns>
        public string GetUsage()
        {
            return $"Usage: {GetCommandScheme()}".Trim();
        }
    }
}
