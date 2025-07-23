using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.UI;
using MLGWorks.Utils.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using static MLGWorks.Utils.Logging.Logger;

using Logger = MLGWorks.Utils.Logging.Logger;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Main entry point for the in‑game developer console.
    /// Listens for toggle key and manages console lifecycle.
    /// </summary>
    [RequireComponent(typeof(ConsoleUI)), DisallowMultipleComponent]
    public class DevConsole : MonoBehaviour
    {
        private void Start()
        {
            CommandManager.RegisterAll();
            Logger.Instance.OnNewLogBatch += handleLogger;
        }

        private void OnDestroy()
        {
            try
            {
                Logger.Instance.OnNewLogBatch -= handleLogger;
            }
            catch { }
        }

        private void handleLogger(List<LogEntry> logBatch)
        {
            foreach (LogEntry log in logBatch)
            {
                LogLevel level = log.Level;
                string msg = log.Message;
                ConsoleUI.Instance?.AppendToOutput(msg, level);
            }
        }

        private void Update()
        {
        }
    }
}
