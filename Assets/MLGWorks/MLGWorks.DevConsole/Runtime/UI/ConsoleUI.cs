using MLGWorks.DevConsole.Runtime.Core;
using MLGWorks.Utils.Logging;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        [SerializeField] private Canvas consoleCanvas;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text outputText;

        [Header("Console Log Colors")]
        [SerializeField] private Color debugColor = new(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color infoColor = Color.white;
        [SerializeField] private Color warningColor = new(1f, 0.64f, 0f);
        [SerializeField] private Color errorColor = new(1f, 0.33f, 0.33f);
        [SerializeField] private Color commandOutputColor = Color.cyan;

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
            outputText.text += FormatMessage(level, message) + "\n";
        }

        public void ClearLogs()
        {
            outputText.text = string.Empty;
        }

        private string FormatMessage(LogLevel? level, string message)
        {
            Color color = level switch
            {
                LogLevel.Debug => levelColors[LogLevel.Debug],
                LogLevel.Info => levelColors[LogLevel.Info],
                LogLevel.Warning => levelColors[LogLevel.Warning],
                LogLevel.Error => levelColors[LogLevel.Error],
                _ => commandOutputColor
            };

            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>";
        }
    }
}
