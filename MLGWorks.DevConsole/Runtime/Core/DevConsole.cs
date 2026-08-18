using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.UI;
using MLGWorks.DevConsole.Runtime.Abstractions;
using MLGWorks.Utils.Logging;
using System;
using System.Collections.Generic;
using MLGWorks.Utils.Patterns.Singletons;
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
        private ICommandHistory _history;
        private IAutocompleteEngine _autocomplete;
        private ICommandExecutor _commandExecutor;
        private ConsoleUI _consoleUI;
        private InputHandler _inputHandler;

        public ConsoleUI ConsoleUI => _consoleUI;

        protected override void Awake()
        {
            base.Awake();

            _consoleUI = GetComponent<ConsoleUI>();
            _inputHandler = GetComponent<InputHandler>();
            _history = new ConsoleHistory();
            _autocomplete = new AutocompleteEngine(CommandManager.Registry);
            _commandExecutor = new CommandManagerExecutor(_consoleUI);

            CommandManager.Registry.RegisterAll();
            _consoleUI.ConfigureServices(_commandExecutor, _history, _autocomplete);
            _inputHandler.Configure(new ConsoleInputSource(), _consoleUI);
            CommandManager.Output = _consoleUI;

            Logger.Instance.OnNewLogBatch += HandleLogger;
        }

        protected override void OnDestroy()
        {
            if (Logger.Instance != null)
            {
                Logger.Instance.OnNewLogBatch -= HandleLogger;
            }

            if (ReferenceEquals(CommandManager.Output, _consoleUI))
            {
                CommandManager.Output = null;
            }

            base.OnDestroy();
        }

        private void HandleLogger(List<LogEntry> logBatch)
        {
            foreach (var log in logBatch)
            {
                _consoleUI.AppendToOutput(log.Message, log.Level);
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
            if (!_commandExecutor.TryExecute(input, out string result))
            {
                // You may want to show command error or unknown command output
                _consoleUI.AppendToOutput(result, LogLevel.Error);
            }
            else if (!string.IsNullOrEmpty(result))
            {
                _consoleUI.AppendToOutput(result, LogLevel.Output);
            }
        }

        /// <summary>
        /// Get autocomplete suggestion for current input
        /// </summary>
        public string GetSuggestion(string input)
        {
            var suggestion = _autocomplete.GetSuggestion(input, out var matchedCmd);
            _autocomplete.SetMatchedCommand(matchedCmd);
            return suggestion;
        }

        /// <summary>
        /// Request autocomplete application
        /// </summary>
        public void RequestAutoComplete() => _autocomplete.RequestAutoComplete();

        /// <summary>
        /// Apply autocomplete if requested
        /// </summary>
        public string PerformAutoComplete(string currentInput)
        {
            return _autocomplete.TryPerformAutoComplete(currentInput) ?? currentInput;
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
            _consoleUI.ClearLogs();
        }
    }
}
