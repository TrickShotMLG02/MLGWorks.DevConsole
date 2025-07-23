using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.UI;
using MLGWorks.Utils.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Captures and processes input from the console input field.
    /// </summary>
    [DisallowMultipleComponent]
    public class InputHandler : MonoBehaviour
    {
        private DevConsoleInputActions _input;
        private List<KeyCode?> _disabledKeys = new();

        private void Awake()
        {
            _input = new DevConsoleInputActions();
            DisableUIKeys();
        }

        private void DisableUIKeys()
        {
            _disabledKeys.Clear();
            _disabledKeys.Add(GetKeyCodeForAction(_input.DevConsole.CommandHistoryPrevious));
            _disabledKeys.Add(GetKeyCodeForAction(_input.DevConsole.CommandHistoryNext));
        }

        private void OnEnable()
        {
            _input.DevConsole.ToggleConsole.performed += OnToggleConsole;
            _input.DevConsole.SubmitCommand.performed += OnSubmitCommand;
            _input.DevConsole.AutoComplete.performed += OnAutoComplete;
            _input.DevConsole.CommandHistoryPrevious.performed += OnCommandHistoryPrevious;
            _input.DevConsole.CommandHistoryNext.performed += OnCommandHistoryNext;
            _input.Enable();
        }

        private void OnDisable()
        {
            _input.DevConsole.ToggleConsole.performed -= OnToggleConsole;
            _input.DevConsole.SubmitCommand.performed -= OnSubmitCommand;
            _input.DevConsole.AutoComplete.performed -= OnAutoComplete;
            _input.DevConsole.CommandHistoryPrevious.performed -= OnCommandHistoryPrevious;
            _input.DevConsole.CommandHistoryNext.performed -= OnCommandHistoryNext;
            _input.Disable();
        }

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
                            if (key.ToString().ToUpper() == keyName.ToUpper())
                                return key;
                        }
                    }
                }
            }
            return null; // no keyboard binding found or failed to parse
        }

        // Disable default UI Events for given keys
        public void OnGUI()
        {
            if (Event.current.isKey)
            {
                if (_disabledKeys.Any(k => k == Event.current.keyCode))
                    Event.current.Use();
            }
        }

        private void OnToggleConsole(InputAction.CallbackContext context)
        {
            if (ConsoleUI.Instance.IsInputFieldFocused)
                return;

            ConsoleUI.Instance.ToggleVisibility();
        }

        private void OnSubmitCommand(InputAction.CallbackContext context)
        {
            ConsoleUI.Instance.OnInputSubmit();
        }

        private void OnAutoComplete(InputAction.CallbackContext context)
        {
            ConsoleUI.Instance.RequestAutoComplete();
        }

        private void OnCommandHistoryPrevious(InputAction.CallbackContext context)
        {
            ConsoleUI.Instance.CommandHistoryPrevious();
        }

        private void OnCommandHistoryNext(InputAction.CallbackContext context)
        {
            ConsoleUI.Instance.CommandHistoryNext();
        }
    }
}
