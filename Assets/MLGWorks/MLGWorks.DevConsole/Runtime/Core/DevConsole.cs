using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.UI;
using MLGWorks.Utils.Logging;
using MLGWorks.Utils.Patterns;
using System;
using System.Collections.Generic;
using UnityEngine;
using static MLGWorks.Utils.Logging.Logger;

using Logger = MLGWorks.Utils.Logging.Logger;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Core singleton managing console logic, input processing, command execution, and log handling.
    /// </summary>
    [RequireComponent(typeof(ConsoleUI)), DisallowMultipleComponent]
    public class DevConsole : Singleton<DevConsole>
    {
        private ConsoleHistory _history;
        private AutocompleteEngine _autocomplete;

        private bool _performAutoComplete = false;
        private CommandInfo _matchedCommand;
        private ConsoleUI consoleUI;

        protected override void Awake()
        {
            base.Awake();

            consoleUI = GetComponent<ConsoleUI>();
            _history = new ConsoleHistory();
            _autocomplete = new AutocompleteEngine();

            CommandManager.RegisterAll();

            Logger.Instance.OnNewLogBatch += HandleLogger;
        }

        protected override void OnDestroy()
        {
            if (Logger.Instance != null)
            {
                Logger.Instance.OnNewLogBatch -= HandleLogger;
            }

            base.OnDestroy();
        }

        private void HandleLogger(List<LogEntry> logBatch)
        {
            foreach (var log in logBatch)
            {
                consoleUI.AppendToOutput(log.Message, log.Level);
            }
        }

        /// <summary>
        /// Called by UI when user submits command input
        /// </summary>
        public void OnInputSubmit(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;

            _history.Add(input);
            SubmitCommand(input);
        }

        private void SubmitCommand(string input)
        {
            if (!CommandManager.TryExecute(input, out string result))
            {
                // You may want to show command error or unknown command output
                consoleUI.AppendToOutput(result, LogLevel.Error);
            }
            else if (!string.IsNullOrEmpty(result))
            {
                consoleUI.AppendToOutput(result, LogLevel.Output);
            }
        }

        /// <summary>
        /// Get autocomplete suggestion for current input
        /// </summary>
        public string GetSuggestion(string input)
        {
            var suggestion = _autocomplete.GetSuggestion(input, out var matchedCmd);
            _matchedCommand = matchedCmd;
            return suggestion;
        }

        /// <summary>
        /// Request autocomplete application
        /// </summary>
        public void RequestAutoComplete() => _performAutoComplete = true;

        /// <summary>
        /// Apply autocomplete if requested
        /// </summary>
        public string PerformAutoComplete(string currentInput)
        {
            if (!_performAutoComplete)
                return currentInput;

            _performAutoComplete = false;

            if (_matchedCommand == null)
                return currentInput;

            // If input already matches or starts with command, do nothing
            if (currentInput.Equals(_matchedCommand.Name, StringComparison.OrdinalIgnoreCase) ||
                currentInput.StartsWith(_matchedCommand.Name + " ", StringComparison.OrdinalIgnoreCase))
            {
                return currentInput;
            }

            return _matchedCommand.Name;
        }

        /// <summary>
        /// Navigate backward in history, returns previous command string
        /// </summary>
        public string HistoryPrevious(string currentInput) => _history.Previous(currentInput);

        /// <summary>
        /// Navigate forward in history, returns next command string or original input
        /// </summary>
        public string HistoryNext() => _history.Next();

        /// <summary>
        /// Clear console logs via UI
        /// </summary>
        public void ClearLogs()
        {
            consoleUI.ClearLogs();
        }
    }
}
