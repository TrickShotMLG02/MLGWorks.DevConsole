using MLGWorks.DevConsole.Runtime.UI;
using Logger = MLGWorks.Utils.Logging.Logger;

namespace MLGWorks.DevConsole.Runtime.Commands
{
    public static class BuiltInCommands
    {
        [Command("help", "Displays all available commands and their usage", "?", "h")]
        public static string Help()
        {
            string result = "";
            foreach (var cmd in CommandManager.CommandInfos)
            {
                result += $"{cmd.GetHelp()}\n";
            }
            return result.TrimEnd();
        }

        [Command("clear", "Clears console output", "cls")]
        public static void Clear()
        {
            ConsoleUI.Instance.ClearLogs();
        }

        [Command("logger.test", "Outputs test logger output")]
        public static void LoggerTest()
        {
            Logger.EmitTestLogs();
        }

        [Command("add", "adds two numbers")]
        public static string Add(float a, float b)
        {
            return (a + b).ToString();
        }

        public static string Get()
        {
            return null;
        }

        public static string Set()
        {
            return null;
        }
    }
}
