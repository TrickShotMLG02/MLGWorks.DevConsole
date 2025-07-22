using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Core;
using MLGWorks.Utils.Logging;
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
    public class ConsoleUI : MonoBehaviour
    {
        public static ConsoleUI Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Canvas consoleCanvas;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text outputText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TMP_Text autocompleteText;

        [Header("Console Log Colors")]
        [SerializeField] private Color debugColor = new Color(0.502f, 0.502f, 0.502f); // #808080
        [SerializeField] private Color infoColor = new Color(0.753f, 0.753f, 0.753f); // #C0C0C0
        [SerializeField] private Color warningColor = new Color(0.976f, 0.780f, 0.310f); // #F9C74F
        [SerializeField] private Color errorColor = new Color(1.000f, 0.176f, 0.188f); // #FF2D30
        [SerializeField] private Color commandColor = new Color(0.565f, 0.745f, 0.427f); // #90BE6D

        [Header("Console Settings")]
        [SerializeField] private int maxLines = 1000;
        public bool enableSuggestions = true;

        // TODO: REMOVE THIS AND MAP IT TO A KEY
        public bool performAutoComplete = false;

        private List<string> logLines = new List<string>();
        private readonly Dictionary<LogLevel, Color> levelColors = new();
        private AutocompleteEngine autocomplete;
        private ConsoleHistory history;

        private void Awake()
        {
            Instance = this;
            consoleCanvas.gameObject.SetActive(false);
            autocomplete = new AutocompleteEngine();
            history = new ConsoleHistory();

            // Init color dictionary
            levelColors[LogLevel.Debug] = debugColor;
            levelColors[LogLevel.Info] = infoColor;
            levelColors[LogLevel.Warning] = warningColor;
            levelColors[LogLevel.Error] = errorColor;
        }

        public void ToggleVisibility()
        {
            consoleCanvas.gameObject.SetActive(!consoleCanvas.gameObject.activeSelf);
            if (consoleCanvas.gameObject.activeSelf)
                inputField.ActivateInputField();
        }

        public void OnInputSubmit()
        {
            var cmd = inputField.text;
            history.Add(cmd);
            GetComponent<InputHandler>().SubmitCommand(cmd);
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }

        public void AppendToOutput(string message, LogLevel? level = null)
        {
            string msg = FormatMessage(level, message);
            logLines.Add(msg);
            if (logLines.Count > maxLines)
            {
                logLines.RemoveAt(0); // remove oldest line
            }

            outputText.text = string.Join("\n", logLines);

            Canvas.ForceUpdateCanvases(); // force layout updates
            scrollRect.verticalNormalizedPosition = 0f; // scroll to bottom
        }

        public void ClearLogs()
        {
            outputText.text = "";
            logLines.Clear();
        }

        private string FormatMessage(LogLevel? level, string message)
        {
            Color color = level switch
            {
                LogLevel.Debug => levelColors[LogLevel.Debug],
                LogLevel.Info => levelColors[LogLevel.Info],
                LogLevel.Warning => levelColors[LogLevel.Warning],
                LogLevel.Error => levelColors[LogLevel.Error],
                _ => commandColor
            };

            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>";
        }

        private void Update()
        {
            CommandInfo matchedCommand = null;

            if (enableSuggestions)
            {
                string input = inputField.text;

                var suggestion = autocomplete.GetSuggestion(input, out matchedCommand);
                if (!string.IsNullOrEmpty(suggestion) && suggestion != input)
                {
                    autocompleteText.text = suggestion;

                    matchedCommand = matchedCommand.Name == input ? null : matchedCommand;
                }
                else
                {
                    autocompleteText.text = string.Empty;
                    matchedCommand = null;
                }
            }

            if (performAutoComplete && matchedCommand != null)
            {
                performAutoComplete = false;
                inputField.text = matchedCommand.Name;

                // move cursor to the end of the auto-completed text
                inputField.caretPosition = inputField.text.Length;
                inputField.selectionAnchorPosition = inputField.text.Length;
                inputField.selectionFocusPosition = inputField.text.Length;
            }
        }
    }
}
