using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.UI;
using MLGWorks.Utils.Logging;
using System.Collections.Generic;
using UnityEngine;
using static MLGWorks.Utils.Logging.Logger;

using Logger = MLGWorks.Utils.Logging.Logger;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Main entry point for the in-game developer console.
    /// Registers all commands and hooks into the logger to display logs in console UI.
    /// Listens for toggle key and manages console lifecycle.
    /// </summary>
    [RequireComponent(typeof(ConsoleUI)), DisallowMultipleComponent]
    public class DevConsole : MonoBehaviour
    {
        /// <summary>
        /// Called on MonoBehaviour start.
        /// Registers all commands and subscribes to logger's new log batch event.
        /// </summary>
        private void Start()
        {
            CommandManager.RegisterAll();
            Logger.Instance.OnNewLogBatch += handleLogger;
        }

        /// <summary>
        /// Called on MonoBehaviour destruction.
        /// Unsubscribes from logger event safely.
        /// </summary>
        private void OnDestroy()
        {
            try
            {
                Logger.Instance.OnNewLogBatch -= handleLogger;
            }
            catch { }
        }

        /// <summary>
        /// Handles batches of log entries by appending them to the console UI output.
        /// </summary>
        /// <param name="logBatch">Batch of new log entries.</param>
        private void handleLogger(List<LogEntry> logBatch)
        {
            foreach (LogEntry log in logBatch)
            {
                LogLevel level = log.Level;
                string msg = log.Message;
                ConsoleUI.Instance?.AppendToOutput(msg, level);
            }
        }
    }
}
