using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Core;
using MLGWorks.Utils.Logging;
using MLGWorks.Utils.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // TODO: REMOVE THIS AND MAP IT TO A KEY
        public bool performAutoComplete = false;

        private List<string> _logLines = new List<string>();
        private readonly Dictionary<LogLevel, Color> _levelColors = new();
        private AutocompleteEngine _autocomplete;
        private ConsoleHistory _history;

        protected override void Awake()
        {
            base.Awake();

            _consoleCanvas.gameObject.SetActive(false);
            _autocomplete = new AutocompleteEngine();
            _history = new ConsoleHistory();

            // Init color dictionary
            _levelColors[LogLevel.Debug] = _debugColor;
            _levelColors[LogLevel.Info] = _infoColor;
            _levelColors[LogLevel.Warning] = _warningColor;
            _levelColors[LogLevel.Error] = _errorColor;
            _levelColors[LogLevel.Command] = _commandColor;
            _levelColors[LogLevel.Output] = _outputColor;
        }

        public void ToggleVisibility()
        {
            _consoleCanvas.gameObject.SetActive(!_consoleCanvas.gameObject.activeSelf);
            if (_consoleCanvas.gameObject.activeSelf)
                _inputField.ActivateInputField();
        }

        public void OnInputSubmit()
        {
            var cmd = _inputField.text;
            _history.Add(cmd);
            GetComponent<InputHandler>().SubmitCommand(cmd);
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }

        public void AppendToOutput(string message, LogLevel? level = null)
        {
            string msg = FormatMessage(level, message);
            _logLines.Add(msg);
            if (_logLines.Count > _maxLinesInBuffer)
            {
                _logLines.RemoveAt(0); // remove oldest line
            }

            _outputText.text = string.Join("\n", _logLines);

            Canvas.ForceUpdateCanvases(); // force layout updates
            _scrollRect.verticalNormalizedPosition = 0f; // scroll to bottom
        }

        public void ClearLogs()
        {
            _outputText.text = "";
            _logLines.Clear();
        }

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

        private void Update()
        {
            CommandInfo matchedCommand = null;

            if (enableSuggestions)
            {
                string input = _inputField.text;

                var suggestion = _autocomplete.GetSuggestion(input, out matchedCommand);
                if (!string.IsNullOrEmpty(suggestion) && suggestion != input)
                {
                    _autocompleteText.text = suggestion;

                    matchedCommand = matchedCommand.Name == input ? null : matchedCommand;
                }
                else
                {
                    _autocompleteText.text = string.Empty;
                    matchedCommand = null;
                }
            }

            if (performAutoComplete && matchedCommand != null)
            {
                performAutoComplete = false;
                _inputField.text = matchedCommand.Name;

                // move cursor to the end of the auto-completed text
                _inputField.caretPosition = _inputField.text.Length;
                _inputField.selectionAnchorPosition = _inputField.text.Length;
                _inputField.selectionFocusPosition = _inputField.text.Length;
            }
        }
    }
}
