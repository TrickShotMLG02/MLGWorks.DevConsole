using System;
using System.Collections.Generic;
using System.Linq;

namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Production input adapter backed by the generated DevConsole input actions.
    /// </summary>
    public sealed class ConsoleInputSource : IConsoleInput
    {
        private readonly DevConsoleInputActions _input;

        public ConsoleInputSource()
        {
            _input = new DevConsoleInputActions();
            _input.DevConsole.ToggleConsole.performed += OnToggleConsole;
            _input.DevConsole.SubmitCommand.performed += OnSubmitCommand;
            _input.DevConsole.AutoComplete.performed += OnAutoComplete;
            _input.DevConsole.CommandHistoryPrevious.performed += OnHistoryPrevious;
            _input.DevConsole.CommandHistoryNext.performed += OnHistoryNext;
        }

        public event Action ToggleConsole;
        public event Action SubmitCommand;
        public event Action AutoComplete;
        public event Action HistoryPrevious;
        public event Action HistoryNext;

        public IReadOnlyList<string> HistoryBindingPaths =>
            _input.DevConsole.CommandHistoryPrevious.bindings
                .Concat(_input.DevConsole.CommandHistoryNext.bindings)
                .Select(binding => binding.effectivePath)
                .ToArray();

        public void Enable() => _input.Enable();
        public void Disable() => _input.Disable();

        public void Dispose()
        {
            _input.DevConsole.ToggleConsole.performed -= OnToggleConsole;
            _input.DevConsole.SubmitCommand.performed -= OnSubmitCommand;
            _input.DevConsole.AutoComplete.performed -= OnAutoComplete;
            _input.DevConsole.CommandHistoryPrevious.performed -= OnHistoryPrevious;
            _input.DevConsole.CommandHistoryNext.performed -= OnHistoryNext;
            _input.Dispose();
        }

        private void OnToggleConsole(UnityEngine.InputSystem.InputAction.CallbackContext context) => ToggleConsole?.Invoke();
        private void OnSubmitCommand(UnityEngine.InputSystem.InputAction.CallbackContext context) => SubmitCommand?.Invoke();
        private void OnAutoComplete(UnityEngine.InputSystem.InputAction.CallbackContext context) => AutoComplete?.Invoke();
        private void OnHistoryPrevious(UnityEngine.InputSystem.InputAction.CallbackContext context) => HistoryPrevious?.Invoke();
        private void OnHistoryNext(UnityEngine.InputSystem.InputAction.CallbackContext context) => HistoryNext?.Invoke();
    }
}
