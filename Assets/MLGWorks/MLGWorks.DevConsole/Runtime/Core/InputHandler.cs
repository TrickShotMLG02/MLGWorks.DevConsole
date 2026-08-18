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
        private IConsoleInputFactory _inputFactory = new ConsoleInputFactory();
        private IConsoleActions _consoleActions;
        private readonly List<KeyCode?> _disabledKeys = new();

        /// <summary>
        /// Initializes input actions and disables conflicting UI keys.
        /// </summary>
        private void Awake()
        {
            _consoleActions = GetComponent<IConsoleActions>();
        }

        /// <summary>
        /// Supplies the input source and console target used by this handler.
        /// This is called by the composition root before Unity enables the component.
        /// </summary>
        public void Configure(IConsoleInput input, IConsoleActions consoleActions,
            IConsoleInputFactory inputFactory = null)
        {
            if (_input != null && !ReferenceEquals(_input, input))
            {
                UnsubscribeFromInput();
                _input.Dispose();
            }

            _input = input;
            _consoleActions = consoleActions;
            _inputFactory = inputFactory ?? _inputFactory;

            if (isActiveAndEnabled)
            {
                SubscribeToInput();
                _input.Enable();
            }

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
            _input ??= _inputFactory.Create();
            _consoleActions ??= GetComponent<IConsoleActions>();
            DisableUIKeys();
            SubscribeToInput();
            _input.Enable();
        }

        /// <summary>
        /// Unsubscribes from input events and disables input actions.
        /// </summary>
        private void OnDisable()
        {
            if (_input == null)
                return;

            UnsubscribeFromInput();
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

        private bool IsConsoleVisible => _consoleActions != null && _consoleActions.IsVisible;

        private void OnToggleConsole()
        {
            if (_consoleActions == null || _consoleActions.IsInputFieldFocused)
                return;

            _consoleActions.ToggleVisibility();
        }

        private void OnSubmitCommand()
        {
            if (IsConsoleVisible)
                _consoleActions.SubmitInput();
        }

        private void OnAutoComplete()
        {
            if (IsConsoleVisible)
                _consoleActions.RequestAutoComplete();
        }

        private void OnCommandHistoryPrevious()
        {
            if (IsConsoleVisible)
                _consoleActions.HistoryPrevious();
        }

        private void OnCommandHistoryNext()
        {
            if (IsConsoleVisible)
                _consoleActions.HistoryNext();
        }

        private void SubscribeToInput()
        {
            _input.ToggleConsole += OnToggleConsole;
            _input.SubmitCommand += OnSubmitCommand;
            _input.AutoComplete += OnAutoComplete;
            _input.HistoryPrevious += OnCommandHistoryPrevious;
            _input.HistoryNext += OnCommandHistoryNext;
        }

        private void UnsubscribeFromInput()
        {
            if (_input == null)
                return;

            _input.ToggleConsole -= OnToggleConsole;
            _input.SubmitCommand -= OnSubmitCommand;
            _input.AutoComplete -= OnAutoComplete;
            _input.HistoryPrevious -= OnCommandHistoryPrevious;
            _input.HistoryNext -= OnCommandHistoryNext;
        }
    }
}
