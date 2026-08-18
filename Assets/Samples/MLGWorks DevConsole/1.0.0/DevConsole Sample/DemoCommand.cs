using MLGWorks.DevConsole.Runtime.Commands;
using System.Linq;

namespace MLGWorks.DevConsole.Examples
{
    public static class DemoCommands
    {
        [Command("echo", "Echoes back the input text")]
        public static string Echo(params string[] input)
        {
            return string.Join(" ", input.ToList());
        }
    }
}
