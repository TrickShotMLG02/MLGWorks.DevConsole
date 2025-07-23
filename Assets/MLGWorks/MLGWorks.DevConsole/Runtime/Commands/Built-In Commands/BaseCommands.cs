using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using MLGWorks.DevConsole.Runtime.UI;
using Logger = MLGWorks.Utils.Logging.Logger;

namespace MLGWorks.DevConsole.Runtime.Commands.BuiltIn
{
    public static class BaseCommands
    {
        [Command("help", "Displays all available commands and their usage", "?", "h")]
        public static string Help()
        {
            var sb = new StringBuilder();
            foreach (var cmd in CommandManager.CommandInfos)
            {
                sb.AppendLine(cmd.GetHelp());
            }
            return sb.ToString().TrimEnd();
        }

        [Command("clear", "Clears console output", "cls")]
        public static void Clear()
        {
            ConsoleUI.Instance.ClearLogs();
        }

        [Command("commands", "Displays all available commands without their usage", "cmds")]
        public static string Commands()
        {
            var sb = new StringBuilder();
            foreach (var cmd in CommandManager.CommandInfos)
            {
                sb.AppendLine(cmd.GetCommandScheme());
            }
            return sb.ToString().TrimEnd();
        }

        [Command("time", "Displays the current system time")]
        public static string Time()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        [Command("uptime", "Displays how long the application has been running")]
        public static string Uptime()
        {
            TimeSpan uptime = TimeSpan.FromSeconds(UnityEngine.Time.realtimeSinceStartup);
            // Format includes days if any
            if (uptime.Days > 0)
                return $"Uptime: {uptime.Days}d {uptime:hh\\:mm\\:ss}";
            return $"Uptime: {uptime:hh\\:mm\\:ss}";
        }

        [Command("platform", "Displays the platform the game is running on")]
        public static string Platform()
        {
#if UNITY_EDITOR
            return "Editor";
#else
            return Application.platform.ToString();
#endif
        }

        [Command("version", "Displays the application version")]
        public static string Version()
        {
            return $"{Application.productName} v{Application.version}";
        }

        [Command("openlogs", "Opens the log folder")]
        public static string OpenLogFolder()
        {
            string path = Logger.Instance.LogDirectory;
            try
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
                return $"Opened log folder: {path}";
#elif UNITY_STANDALONE_OSX
                Process.Start("open", path);
                return $"Opened log folder: {path}";
#elif UNITY_STANDALONE_LINUX
                Process.Start("xdg-open", path);
                return $"Opened log folder: {path}";
#else
                return $"Log folder path: {path}";
#endif
            }
            catch (Exception e)
            {
                return $"Failed to open folder: {e.Message}";
            }
        }
    }
}
