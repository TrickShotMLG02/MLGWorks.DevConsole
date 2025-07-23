using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Core;
using MLGWorks.Utils.Logging;
using MLGWorks.Utils.Patterns;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = MLGWorks.Utils.Logging.Logger;

namespace MLGWorks.DevConsole.Runtime.UI
{
    [Serializable]
    public struct LogLevelColor
    {
        public LogLevel? Level;
        public Color Color;

        public string GetHex() => ColorUtility.ToHtmlStringRGB(Color);
    }

    /// <summary>
    /// Singleton managing the console UI display and input.
    /// Uses ScrollbackBuffer to limit displayed log lines.
    /// </summary>
    [RequireComponent(typeof(InputHandler)), DisallowMultipleComponent]
    public class ConsoleUI : Singleton<ConsoleUI>
    {
        [Header("References")]
        [SerializeField] private Canvas _consoleCanvas;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _outputText;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private TMP_Text _autocompleteText;

        [Header("Console Log Colors")]
        [SerializeField] private Color _debugColor = new Color(0.502f, 0.502f, 0.502f); // #808080
        [SerializeField] private Color _infoColor = new Color(0.753f, 0.753f, 0.753f); // #C0C0C0
        [SerializeField] private Color _warningColor = new Color(0.976f, 0.780f, 0.310f); // #F9C74F
        [SerializeField] private Color _errorColor = new Color(1.000f, 0.176f, 0.188f); // #FF2D30
        [SerializeField] private Color _outputColor = new Color(0.565f, 0.745f, 0.427f); // #90BE6D
        [SerializeField] private Color _commandColor = new Color(0f, 0.847f, 1f); // #00D8FF

        [Header("Console Settings")]
        [SerializeField] private string _commandPrefix = "> ";
        public string CommandPrefix => _commandPrefix;
        [SerializeField] private int _maxLinesInBuffer = 1000;
        public bool enableSuggestions = true;
        private bool _performAutoComplete = false;
        private CommandInfo _matchedCommand;
        [SerializeField] private Color _invalidCommandColor = new Color(0.8235f, 0.0157f, 0.1765f); // #D2042D

        // ScrollbackBuffer replaces the previous List<string> buffer for managing log lines
        private ScrollbackBuffer _logBuffer;

        // Dictionary for quick lookup of log level colors
        private readonly Dictionary<LogLevel, Color> _levelColors = new();

        private AutocompleteEngine _autocomplete;
        private ConsoleHistory _history;

        private Color _originalInputFieldColor;

        public bool IsInputFieldFocused => _inputField.isFocused;

        /// <summary>
        /// Initializes the console, disables it by default, and sets up necessary components.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Disable console UI on start
            _consoleCanvas.gameObject.SetActive(false);

            // Initialize autocomplete engine and command history
            _autocomplete = new AutocompleteEngine();
            _history = new ConsoleHistory();

            // Initialize the ScrollbackBuffer with max lines
            _logBuffer = new ScrollbackBuffer(_maxLinesInBuffer);

            // Setup the color dictionary for log levels
            _levelColors[LogLevel.Debug] = _debugColor;
            _levelColors[LogLevel.Info] = _infoColor;
            _levelColors[LogLevel.Warning] = _warningColor;
            _levelColors[LogLevel.Error] = _errorColor;
            _levelColors[LogLevel.Command] = _commandColor;
            _levelColors[LogLevel.Output] = _outputColor;

            _originalInputFieldColor = _inputField.textComponent.color;
        }

        /// <summary>
        /// Toggles the console visibility on/off.
        /// </summary>
        public void ToggleVisibility()
        {
            _consoleCanvas.gameObject.SetActive(!_consoleCanvas.gameObject.activeSelf);
            if (_consoleCanvas.gameObject.activeSelf)
                _inputField.ActivateInputField();
        }

        /// <summary>
        /// Handles input submission: add to history, process command, reset input.
        /// </summary>
        public void OnInputSubmit()
        {
            var cmd = _inputField.text;

            if (string.IsNullOrEmpty(cmd))
                return;

            _history.Add(cmd);
            SubmitCommand(cmd);
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }

        /// <summary>
        /// Appends a new line to the output text using ScrollbackBuffer to limit lines.
        /// </summary>
        /// <param name="message">The log or command message.</param>
        /// <param name="level">Optional log level to determine color.</param>
        public void AppendToOutput(string message, LogLevel? level = null)
        {
            // Split the message by line breaks to handle multiline messages
            var lines = message.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            // Format each line with the log level color
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = FormatMessage(level, lines[i]);
            }

            // Rejoin the formatted lines with line breaks
            string formattedMessage = string.Join("\n", lines);

            // Add message to the scrollback buffer (auto trims if over limit)
            _logBuffer.Add(formattedMessage);

            // Join all buffered lines and display
            _outputText.text = string.Join("\n", _logBuffer.GetLines());

            // Force UI update and scroll to bottom
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        /// <summary>
        /// Clears the console output and resets the scrollback buffer.
        /// </summary>
        public void ClearLogs()
        {
            _logBuffer = new ScrollbackBuffer(_maxLinesInBuffer);
            _outputText.text = "";
        }

        /// <summary>
        /// Formats the message text with color tags based on log level.
        /// </summary>
        private string FormatMessage(LogLevel? level, string message)
        {
            Color color = level switch
            {
                LogLevel.Debug => _levelColors[LogLevel.Debug],
                LogLevel.Info => _levelColors[LogLevel.Info],
                LogLevel.Warning => _levelColors[LogLevel.Warning],
                LogLevel.Error => _levelColors[LogLevel.Error],
                LogLevel.Command => _levelColors[LogLevel.Command],
                LogLevel.Output => _levelColors[LogLevel.Output],
                _ => Color.white
            };

            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>";
        }

        /// <summary>
        /// Navigates back in command history and updates the input field.
        /// </summary>
        public void CommandHistoryPrevious()
        {
            _inputField.text = _history.Previous(_inputField.text);
            _inputField.caretPosition = _inputField.text.Length;
        }

        /// <summary>
        /// Navigates forward in command history and updates the input field.
        /// </summary>
        public void CommandHistoryNext()
        {
            _inputField.text = _history.Next();
            _inputField.caretPosition = _inputField.text.Length;
        }

        /// <summary>
        /// Executes the command string and appends output if any.
        /// </summary>
        private void SubmitCommand(string input)
        {
            string result = null;
            CommandManager.TryExecute(input, out result);

            if (!string.IsNullOrEmpty(result))
                ConsoleUI.Instance.AppendToOutput(result, LogLevel.Output);
        }

        /// <summary>
        /// Called every frame to handle autocomplete suggestions and input logic.
        /// </summary>
        private void Update()
        {
            PerformSuggestion();
            PerformAutoComplete();
        }

        /// <summary>
        /// Generates autocomplete suggestions for the current input text.
        /// </summary>
        private void PerformSuggestion()
        {
            if (_inputField.textComponent.color != _originalInputFieldColor)
                _inputField.textComponent.color = _originalInputFieldColor;

            if (!enableSuggestions)
                return;

            string input = _inputField.text;

            if (string.IsNullOrWhiteSpace(input))
            {
                _autocompleteText.text = string.Empty;
                _matchedCommand = null;
                return;
            }

            var suggestion = _autocomplete.GetSuggestion(input, out var matchedCommand);
            if (!string.IsNullOrEmpty(suggestion) && suggestion != input)
            {
                _autocompleteText.text = suggestion;
                _matchedCommand = matchedCommand.Name == input ? null : matchedCommand;
            }
            else
            {
                _autocompleteText.text = string.Empty;
                _matchedCommand = null;

                if (suggestion != input)
                    _inputField.textComponent.color = _invalidCommandColor;
            }
        }

        /// <summary>
        /// Requests autocomplete to apply on next Update.
        /// </summary>
        public void RequestAutoComplete()
        {
            _performAutoComplete = true;
        }

        /// <summary>
        /// Applies the autocomplete text to input field if available.
        /// </summary>
        private void PerformAutoComplete()
        {
            if (!_performAutoComplete)
                return;

            _performAutoComplete = false;

            if (_matchedCommand == null)
                return;

            string input = _inputField.text.TrimStart();

            // If input already exactly matches or starts with the command, do nothing
            if (input.Equals(_matchedCommand.Name, StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith(_matchedCommand.Name + " ", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _inputField.text = _matchedCommand.Name;

            // Move caret to end of input field
            _inputField.caretPosition = _inputField.text.Length;
            _inputField.selectionAnchorPosition = _inputField.text.Length;
            _inputField.selectionFocusPosition = _inputField.text.Length;
        }
    }
}
