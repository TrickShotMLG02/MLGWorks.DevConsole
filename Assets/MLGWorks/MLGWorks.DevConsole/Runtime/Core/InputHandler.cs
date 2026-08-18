using System;
using System.Collections.Generic;
using System.Linq;
using MLGWorks.DevConsole.Runtime.Abstractions;
using UnityEngine;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Captures and processes input from the console input field.
    /// Manages input bindings and disables conflicting UI keys.
    /// </summary>
    [DisallowMultipleComponent]
    public class InputHandler : MonoBehaviour
    {
        private IConsoleInput _input;
        private readonly List<KeyCode?> _disabledKeys = new();

        /// <summary>
        /// Initializes input actions and disables conflicting UI keys.
        /// </summary>
        private void Awake()
        {
            _input = new ConsoleInputSource();
            DisableUIKeys();
        }

        /// <summary>
        /// Populates the list of keys for which default UI events should be disabled.
        /// </summary>
        private void DisableUIKeys()
        {
            _disabledKeys.Clear();

            foreach (var bindingPath in _input.HistoryBindingPaths)
            {
                var keyCode = GetKeyCodeForBindingPath(bindingPath);
                if (keyCode.HasValue && !_disabledKeys.Contains(keyCode))
                    _disabledKeys.Add(keyCode);
            }
        }

        /// <summary>
        /// Subscribes to input events and enables input actions.
        /// </summary>
        private void OnEnable()
        {
            _input.ToggleConsole += OnToggleConsole;
            _input.SubmitCommand += OnSubmitCommand;
            _input.AutoComplete += OnAutoComplete;
            _input.HistoryPrevious += OnCommandHistoryPrevious;
            _input.HistoryNext += OnCommandHistoryNext;
            _input.Enable();
        }

        /// <summary>
        /// Unsubscribes from input events and disables input actions.
        /// </summary>
        private void OnDisable()
        {
            if (_input == null)
                return;

            _input.ToggleConsole -= OnToggleConsole;
            _input.SubmitCommand -= OnSubmitCommand;
            _input.AutoComplete -= OnAutoComplete;
            _input.HistoryPrevious -= OnCommandHistoryPrevious;
            _input.HistoryNext -= OnCommandHistoryNext;
            _input.Disable();
        }

        private void OnDestroy()
        {
            _input?.Dispose();
        }

        /// <summary>
        /// Attempts to find a keyboard KeyCode from a binding path.
        /// </summary>
        private KeyCode? GetKeyCodeForBindingPath(string bindingPath)
        {
            if (string.IsNullOrEmpty(bindingPath) || !bindingPath.StartsWith("<Keyboard>"))
                return null;

            var parts = bindingPath.Split('/');
            if (parts.Length < 2)
                return null;

            string keyName = parts[1];
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (key.ToString().Equals(keyName, StringComparison.OrdinalIgnoreCase))
                    return key;
            }

            return null;
        }

        /// <summary>
        /// Disables Unity UI events for certain keys to avoid conflicts with the console.
        /// </summary>
        public void OnGUI()
        {
            if (IsConsoleVisible && Event.current.isKey &&
                _disabledKeys.Any(k => k == Event.current.keyCode))
            {
                Event.current.Use();
            }
        }

        private static bool IsConsoleVisible =>
            DevConsole.Instance != null &&
            DevConsole.Instance.ConsoleUI != null &&
            DevConsole.Instance.ConsoleUI.IsVisible;

        private void OnToggleConsole()
        {
            if (DevConsole.Instance.ConsoleUI.IsInputFieldFocused)
                return;

            DevConsole.Instance.ConsoleUI.ToggleVisibility();
        }

        private void OnSubmitCommand()
        {
            if (IsConsoleVisible)
                DevConsole.Instance.ConsoleUI.OnInputSubmit();
        }

        private void OnAutoComplete()
        {
            if (IsConsoleVisible)
                DevConsole.Instance.ConsoleUI.RequestAutoComplete();
        }

        private void OnCommandHistoryPrevious()
        {
            if (IsConsoleVisible)
                DevConsole.Instance.ConsoleUI.CommandHistoryPrevious();
        }

        private void OnCommandHistoryNext()
        {
            if (IsConsoleVisible)
                DevConsole.Instance.ConsoleUI.CommandHistoryNext();
        }
    }
}
