using MLGWorks.DevConsole.Runtime.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Provides autocomplete suggestion including typed part and missing remainder.
    /// </summary>
    public class AutocompleteEngine
    {
        public string GetSuggestion(string input)
        {
            CommandInfo cmd = null;
            return GetSuggestion(input, out cmd);
        }

        /// <summary>
        /// Returns full suggestion text including typed parts, and missing parts.
        /// </summary>
        public string GetSuggestion(string input, out CommandInfo matchedCommand)
        {
            matchedCommand = null;

            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var tokens = input.Trim().Split(' ');
            string typedCommand = tokens[0];
            string[] typedArgs = tokens.Skip(1).ToArray();

            // Find matching command by prefix
            var matchKVP = CommandManager.Commands
                .FirstOrDefault(pair => pair.Key.StartsWith(typedCommand, StringComparison.OrdinalIgnoreCase));

            var matchedAlias = matchKVP.Key;
            var match = matchKVP.Value;

            matchedCommand = match;

            if (match == null)
                return string.Empty; // no match, just return input

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

            if (parameters.Length == 1 &&
                Attribute.IsDefined(parameters[0], typeof(ParamArrayAttribute)))
            {
                // Special case: params string[]
                if (typedCount == 0)
                {
                    sb.Append(" <string[]>");
                }
                // If user already typed args, no need to suggest further parameters
                return sb.ToString();
            }

            // Append missing parameters (those not typed yet)
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
    }
}
