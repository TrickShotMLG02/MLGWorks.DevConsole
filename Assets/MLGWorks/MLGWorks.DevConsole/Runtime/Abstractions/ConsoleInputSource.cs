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
        private bool _disposed;

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

        public void Enable()
        {
            ThrowIfDisposed();
            _input.Enable();
        }

        public void Disable()
        {
            if (!_disposed)
                _input.DevConsole.Disable();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            // The generated wrapper asserts in its finalizer unless the action
            // map is disabled before the asset is destroyed.
            _input.DevConsole.Disable();
            _input.DevConsole.ToggleConsole.performed -= OnToggleConsole;
            _input.DevConsole.SubmitCommand.performed -= OnSubmitCommand;
            _input.DevConsole.AutoComplete.performed -= OnAutoComplete;
            _input.DevConsole.CommandHistoryPrevious.performed -= OnHistoryPrevious;
            _input.DevConsole.CommandHistoryNext.performed -= OnHistoryNext;
            _input.Dispose();
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ConsoleInputSource));
        }

        private void OnToggleConsole(UnityEngine.InputSystem.InputAction.CallbackContext context) => ToggleConsole?.Invoke();
        private void OnSubmitCommand(UnityEngine.InputSystem.InputAction.CallbackContext context) => SubmitCommand?.Invoke();
        private void OnAutoComplete(UnityEngine.InputSystem.InputAction.CallbackContext context) => AutoComplete?.Invoke();
        private void OnHistoryPrevious(UnityEngine.InputSystem.InputAction.CallbackContext context) => HistoryPrevious?.Invoke();
        private void OnHistoryNext(UnityEngine.InputSystem.InputAction.CallbackContext context) => HistoryNext?.Invoke();
    }
}
