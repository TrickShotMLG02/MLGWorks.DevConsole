using MLGWorks.DevConsole.Runtime.Commands;
using System.Collections.Generic;
using System.Linq;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Provides command name autocomplete based on registered commands.
    /// </summary>
    public class AutocompleteEngine
    {
        public IEnumerable<string> GetSuggestions(string prefix)
        {
            return CommandManager.Commands.Values
                .Where(cmd => cmd.Name.StartsWith(prefix))
                .Select(cmd => cmd.Name);
        }
    }
}
