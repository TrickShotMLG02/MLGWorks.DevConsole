using MLGWorks.DevConsole.Runtime.UI;
using Logger = MLGWorks.Utils.Logging.Logger;

namespace MLGWorks.DevConsole.Runtime.Commands
{
    public static class BuiltInCommands
    {
        [Command("help", "Clears console output", "?", "h")]
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
    }
}
