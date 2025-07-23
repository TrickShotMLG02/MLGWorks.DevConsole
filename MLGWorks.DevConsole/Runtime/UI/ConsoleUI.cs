using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Core;
using MLGWorks.Utils.Logging;
using MLGWorks.Utils.Patterns;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MLGWorks.DevConsole.Runtime.UI
{
    /// <summary>
    /// Struct representing a mapping between a <see cref="LogLevel"/> and a <see cref="Color"/>.
    /// </summary>
    [Serializable]
    public struct LogLevelColor
    {
        /// <summary>
        /// The optional log level this color is associated with.
        /// </summary>
        public LogLevel? Level;

        /// <summary>
        /// The color to use for the log level.
        /// </summary>
        public Color Color;

        /// <summary>
        /// Returns the color as a hex string in RGB format, suitable for Unity rich text.
        /// </summary>
        /// <returns>Hex string representing the color.</returns>
        public string GetHex() => ColorUtility.ToHtmlStringRGB(Color);
    }

    /// <summary>
    /// Singleton responsible for managing the developer console UI, including input handling,
    /// displaying logs with color coding, command history, autocomplete, and toggling visibility.
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

        /// <summary>
        /// Prefix used before user commands in the console UI.
        /// </summary>
        public string CommandPrefix => _commandPrefix;

        [SerializeField] private int _maxLinesInBuffer = 1000;

        /// <summary>
        /// Enables or disables command suggestions/autocomplete.
        /// </summary>
        public bool enableSuggestions = true;

        private bool _performAutoComplete = false;
        private CommandInfo _matchedCommand;

        [SerializeField] private Color _invalidCommandColor = new Color(0.8235f, 0.0157f, 0.1765f); // #D2042D

        private List<string> _logLines = new List<string>();
        private readonly Dictionary<LogLevel, Color> _levelColors = new();
        private AutocompleteEngine _autocomplete;
        private ConsoleHistory _history;

        private Color _originalInputFieldColor;

        /// <summary>
        /// Returns whether the input field currently has keyboard focus.
        /// </summary>
        public bool IsInputFieldFocused => _inputField.isFocused;

        /// <summary>
        /// Initializes the singleton and related state on awake.
        /// Sets up color mappings and initializes history and autocomplete systems.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _consoleCanvas.gameObject.SetActive(false);
            _autocomplete = new AutocompleteEngine();
            _history = new ConsoleHistory();

            // Initialize color dictionary for each log level
            _levelColors[LogLevel.Debug] = _debugColor;
            _levelColors[LogLevel.Info] = _infoColor;
            _levelColors[LogLevel.Warning] = _warningColor;
            _levelColors[LogLevel.Error] = _errorColor;
            _levelColors[LogLevel.Command] = _commandColor;
            _levelColors[LogLevel.Output] = _outputColor;

            _originalInputFieldColor = _inputField.textComponent.color;
        }

        /// <summary>
        /// Toggles the visibility of the console UI.
        /// When shown, focuses the input field for user commands.
        /// </summary>
        public void ToggleVisibility()
        {
            _consoleCanvas.gameObject.SetActive(!_consoleCanvas.gameObject.activeSelf);
            if (_consoleCanvas.gameObject.activeSelf)
                _inputField.ActivateInputField();
        }

        /// <summary>
        /// Called when the user submits a command input.
        /// Adds command to history, executes it, and resets input field.
        /// </summary>
        public void OnInputSubmit()
        {
            var cmd = _inputField.text;

            if (cmd == "")
                return;

            _history.Add(cmd);
            SubmitCommand(cmd);
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }

        /// <summary>
        /// Appends a formatted message to the console output area with optional color coding by log level.
        /// </summary>
        /// <param name="message">The message to append.</param>
        /// <param name="level">Optional log level for color coding.</param>
        public void AppendToOutput(string message, LogLevel? level = null)
        {
            string msg = FormatMessage(level, message);
            _logLines.Add(msg);
            if (_logLines.Count > _maxLinesInBuffer)
            {
                _logLines.RemoveAt(0); // remove oldest line to maintain buffer size
            }

            _outputText.text = string.Join("\n", _logLines);

            // Force layout update and scroll to bottom
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        /// <summary>
        /// Clears all console output lines.
        /// </summary>
        public void ClearLogs()
        {
            _outputText.text = "";
            _logLines.Clear();
        }

        /// <summary>
        /// Formats a log message with the color corresponding to the given log level.
        /// </summary>
        /// <param name="level">Log level to determine color.</param>
        /// <param name="message">Message text.</param>
        /// <returns>Colored message string in Unity rich text format.</returns>
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
        /// Replaces the input field text with the previous command from history and moves the caret to the end.
        /// </summary>
        public void CommandHistoryPrevious()
        {
            _inputField.text = _history.Previous(_inputField.text);
            _inputField.caretPosition = _inputField.text.Length;
        }

        /// <summary>
        /// Replaces the input field text with the next command from history and moves the caret to the end.
        /// </summary>
        public void CommandHistoryNext()
        {
            _inputField.text = _history.Next();
            _inputField.caretPosition = _inputField.text.Length;
        }

        /// <summary>
        /// Executes the given command string through the CommandManager and appends the result to the output.
        /// </summary>
        /// <param name="input">Command string to execute.</param>
        private void SubmitCommand(string input)
        {
            string result = null;
            CommandManager.TryExecute(input, out result);

            if (result != null)
                ConsoleUI.Instance.AppendToOutput(result, LogLevel.Output);
        }

        /// <summary>
        /// Unity update loop, performs suggestion and autocomplete processing each frame.
        /// </summary>
        private void Update()
        {
            PerformSuggestion();
            PerformAutoComplete();
        }

        /// <summary>
        /// Updates the autocomplete suggestion UI and highlights input if the current command is invalid.
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
        /// Requests the console to perform autocomplete on the current input on the next frame.
        /// </summary>
        public void RequestAutoComplete()
        {
            _performAutoComplete = true;
        }

        /// <summary>
        /// Performs autocomplete by replacing the input with the matched command name if appropriate.
        /// </summary>
        private void PerformAutoComplete()
        {
            if (!_performAutoComplete)
                return;

            _performAutoComplete = false;

            if (_matchedCommand == null)
                return;

            string input = _inputField.text.TrimStart();

            // If input exactly matches command or starts with it plus a space, do not overwrite
            if (input.Equals(_matchedCommand.Name, StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith(_matchedCommand.Name + " ", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _inputField.text = _matchedCommand.Name;

            // Move caret to the end of the text
            _inputField.caretPosition = _inputField.text.Length;
            _inputField.selectionAnchorPosition = _inputField.text.Length;
            _inputField.selectionFocusPosition = _inputField.text.Length;
        }
    }
}
