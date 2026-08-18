using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Abstractions;
using System;
using System.Linq;
using System.Text;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Provides autocomplete suggestions for console commands.
    /// Suggests the full command including typed parts and the missing remainder.
    /// Also manages applying autocomplete.
    /// </summary>
    public class AutocompleteEngine : IAutocompleteEngine
    {
        private readonly ICommandRegistry _commandRegistry;
        private CommandInfo _matchedCommand;
        private string _matchedAlias;
        private bool _performAutoComplete;

        public AutocompleteEngine(ICommandRegistry commandRegistry = null)
        {
            _commandRegistry = commandRegistry ?? CommandManager.Registry;
        }

        /// <summary>
        /// Gets an autocomplete suggestion for the given input.
        /// </summary>
        /// <param name="input">Partial input typed by the user.</param>
        /// <param name="matchedCommand">Outputs the matched <see cref="CommandInfo"/> if found; otherwise null.</param>
        /// <returns>Suggested full command string including typed and missing parts.</returns>
        public string GetSuggestion(string input)
        {
            CommandInfo cmd = null;
            return GetSuggestion(input, out cmd);
        }

        /// <summary>
        /// Gets an autocomplete suggestion and the matched command info for the given input.
        /// </summary>
        public string GetSuggestion(string input, out CommandInfo matchedCommand)
        {
            matchedCommand = null;

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var tokens = input.Trim().Split(' ');
            string typedCommand = tokens[0];
            string[] typedArgs = tokens.Skip(1).ToArray();

            // Find matching command by prefix, case-insensitive
            var matchKVP = _commandRegistry.Commands
                .FirstOrDefault(pair => pair.Key.StartsWith(typedCommand, StringComparison.OrdinalIgnoreCase));

            var matchedAlias = matchKVP.Key;
            var match = matchKVP.Value;

            _matchedAlias = matchedAlias;
            matchedCommand = match;

            if (match == null)
                return string.Empty; // No match found, return empty suggestion

            var method = match.Method;
            var parameters = method.GetParameters();

            var sb = new StringBuilder();

            // Append typed command part as is
            sb.Append(typedCommand);

            // Append remaining command letters (missing part)
            string remainingCommand = matchedAlias.Substring(typedCommand.Length);
            if (!string.IsNullOrEmpty(remainingCommand))
                sb.Append(remainingCommand);

            // Append typed arguments with spaces
            foreach (var arg in typedArgs)
                sb.Append(" ").Append(arg);

            int typedCount = typedArgs.Length;
            int paramCount = parameters.Length;

            // Special case: if method has a single params string[] parameter
            if (parameters.Length == 1 &&
                Attribute.IsDefined(parameters[0], typeof(ParamArrayAttribute)))
            {
                if (typedCount == 0)
                {
                    sb.Append(" <string[]>");
                }
                // If user already typed args, no further suggestions needed
                return sb.ToString();
            }

            // Append missing parameters as placeholders, showing optionality and type
            for (int i = typedCount; i < paramCount; i++)
            {
                var param = parameters[i];
                string typeName = Utils.GetReadableTypeName(param.ParameterType);

                if (Attribute.IsDefined(param, typeof(ParamArrayAttribute)))
                    typeName = $"{Utils.GetReadableTypeName(param.ParameterType.GetElementType())}[]";

                sb.Append(" ");
                sb.Append(param.IsOptional ? $"[{param.Name}: {typeName}]" : $"<{param.Name}: {typeName}>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Requests autocomplete to be performed on next update.
        /// </summary>
        public void RequestAutoComplete()
        {
            _performAutoComplete = true;
        }

        /// <summary>
        /// Tries to perform autocomplete based on current input.
        /// Returns new input string if autocomplete applied, otherwise null.
        /// </summary>
        public string TryPerformAutoComplete(string currentInput)
        {
            if (!_performAutoComplete)
                return null;

            _performAutoComplete = false;

            if (_matchedCommand == null || string.IsNullOrWhiteSpace(_matchedAlias))
                return null;

            string input = currentInput.TrimStart();

            // If input already exactly matches or starts with the matched alias, do nothing
            if (input.Equals(_matchedAlias, StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith(_matchedAlias + " ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return _matchedAlias;
        }

        /// <summary>
        /// Sets the matched command for future autocomplete application.
        /// </summary>
        public void SetMatchedCommand(CommandInfo matchedCommand)
        {
            _matchedCommand = matchedCommand;
        }
    }
}
