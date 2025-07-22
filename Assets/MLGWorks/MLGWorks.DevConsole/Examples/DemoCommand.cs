using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.UI;
using System.Linq;

namespace MLGWorks.DevConsole.Examples
{
    public static class DemoCommands
    {
        [Command("echo", "Echoes back the input text")]
        public static void Echo(string input)
        {
            var words = input.Split(' ').ToList();
            words.RemoveAt(0);
            var res = string.Join(" ", words);
            ConsoleUI.Instance.AppendToOutput(res);
        }
    }
}
