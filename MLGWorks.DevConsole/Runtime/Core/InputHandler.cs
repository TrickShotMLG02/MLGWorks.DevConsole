using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Captures and processes input from the console input field.
    /// Manages input bindings and disables conflicting UI keys.
    /// </summary>
    [DisallowMultipleComponent]
    public class InputHandler : MonoBehaviour
    {
        private DevConsoleInputActions _input;
        private List<KeyCode?> _disabledKeys = new();

        /// <summary>
        /// Initializes input actions and disables conflicting UI keys.
        /// </summary>
        private void Awake()
        {
            _input = new DevConsoleInputActions();
            DisableUIKeys();
        }

        /// <summary>
        /// Populates the list of keys for which default UI events should be disabled.
        /// </summary>
        private void DisableUIKeys()
        {
            _disabledKeys.Clear();
            _disabledKeys.Add(GetKeyCodeForAction(_input.DevConsole.CommandHistoryPrevious));
            _disabledKeys.Add(GetKeyCodeForAction(_input.DevConsole.CommandHistoryNext));
        }

        /// <summary>
        /// Subscribes to input events and enables input actions.
        /// </summary>
        private void OnEnable()
        {
            _input.DevConsole.ToggleConsole.performed += OnToggleConsole;
            _input.DevConsole.SubmitCommand.performed += OnSubmitCommand;
            _input.DevConsole.AutoComplete.performed += OnAutoComplete;
            _input.DevConsole.CommandHistoryPrevious.performed += OnCommandHistoryPrevious;
            _input.DevConsole.CommandHistoryNext.performed += OnCommandHistoryNext;
            _input.Enable();
        }

        /// <summary>
        /// Unsubscribes from input events and disables input actions.
        /// </summary>
        private void OnDisable()
        {
            _input.DevConsole.ToggleConsole.performed -= OnToggleConsole;
            _input.DevConsole.SubmitCommand.performed -= OnSubmitCommand;
            _input.DevConsole.AutoComplete.performed -= OnAutoComplete;
            _input.DevConsole.CommandHistoryPrevious.performed -= OnCommandHistoryPrevious;
            _input.DevConsole.CommandHistoryNext.performed -= OnCommandHistoryNext;
            _input.Disable();
        }

        /// <summary>
        /// Attempts to find the first keyboard KeyCode bound to a given input action.
        /// </summary>
        /// <param name="action">Input action to examine.</param>
        /// <returns>The KeyCode if found; otherwise null.</returns>
        private KeyCode? GetKeyCodeForAction(InputAction action)
        {
            // Find first binding on keyboard device
            foreach (var binding in action.bindings)
            {
                if (binding.effectivePath.StartsWith("<Keyboard>"))
                {
                    // Example path: "<Keyboard>/p"
                    var parts = binding.effectivePath.Split('/');
                    if (parts.Length > 1)
                    {
                        string keyName = parts[1]; // e.g. "p"

                        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                        {
                            if (key.ToString().Equals(keyName, StringComparison.OrdinalIgnoreCase))
                                return key;
                        }
                    }
                }
            }
            return null; // no keyboard binding found or failed to parse
        }

        /// <summary>
        /// Disables Unity UI events for certain keys to avoid conflicts with the console.
        /// </summary>
        public void OnGUI()
        {
            if (IsConsoleVisible && Event.current.isKey)
            {
                if (_disabledKeys.Any(k => k == Event.current.keyCode))
                    Event.current.Use();
            }
        }

        private static bool IsConsoleVisible =>
            DevConsole.Instance != null &&
            DevConsole.Instance.ConsoleUI != null &&
            DevConsole.Instance.ConsoleUI.IsVisible;

        /// <summary>
        /// Handles toggling the console UI visibility.
        /// Does nothing if input field is focused to avoid accidental toggling.
        /// </summary>
        /// <param name="context">Input callback context.</param>
        private void OnToggleConsole(InputAction.CallbackContext context)
        {
            if (DevConsole.Instance.ConsoleUI.IsInputFieldFocused)
                return;

            DevConsole.Instance.ConsoleUI.ToggleVisibility();
        }

        /// <summary>
        /// Submits the current input in the console UI.
        /// </summary>
        /// <param name="context">Input callback context.</param>
        private void OnSubmitCommand(InputAction.CallbackContext context)
        {
            if (!IsConsoleVisible)
                return;

            DevConsole.Instance.ConsoleUI.OnInputSubmit();
        }

        /// <summary>
        /// Requests autocomplete from the console UI.
        /// </summary>
        /// <param name="context">Input callback context.</param>
        private void OnAutoComplete(InputAction.CallbackContext context)
        {
            if (!IsConsoleVisible)
                return;

            DevConsole.Instance.ConsoleUI.RequestAutoComplete();
        }

        /// <summary>
        /// Navigates to the previous command in history.
        /// </summary>
        /// <param name="context">Input callback context.</param>
        private void OnCommandHistoryPrevious(InputAction.CallbackContext context)
        {
            if (!IsConsoleVisible)
                return;

            DevConsole.Instance.ConsoleUI.CommandHistoryPrevious();
        }

        /// <summary>
        /// Navigates to the next command in history.
        /// </summary>
        /// <param name="context">Input callback context.</param>
        private void OnCommandHistoryNext(InputAction.CallbackContext context)
        {
            if (!IsConsoleVisible)
                return;

            DevConsole.Instance.ConsoleUI.CommandHistoryNext();
        }
    }
}
