using System;
using System.Collections.Generic;
using MLGWorks.DevConsole.Runtime.Abstractions;
using UnityEngine;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Maintains a history of entered console commands and allows navigation
    /// through previous and next commands, preserving temporary unsaved input.
    /// </summary>
    public class ConsoleHistory : ICommandHistory
    {
        private readonly List<string> _history = new List<string>();
        private int _index = -1;

        private string _temporaryInput = string.Empty;
        private bool _savedTemporaryInput = false;

        /// <summary>
        /// Adds a new command to the history if it's not a duplicate of the last entry.
        /// Resets the navigation index.
        /// </summary>
        /// <param name="command">The command string to add.</param>
        public void Add(string command)
        {
            if (_history.Count == 0 || _history[_history.Count - 1] != command)
            {
                _history.Add(command);
                ResetIndex();
            }
            _index = _history.Count;
        }

        /// <summary>
        /// Navigates to the previous command in history.
        /// Saves the current input before navigating.
        /// </summary>
        /// <param name="currentInput">The current text input before navigation.</param>
        /// <returns>The previous command from history or empty if none.</returns>
        public string Previous(string currentInput)
        {
            if (_history.Count == 0) return string.Empty;

            // Save the current input before navigating
            SaveCurrentInput(currentInput);

            _index = Mathf.Clamp(_index - 1, 0, _history.Count - 1);
            return _history[_index];
        }

        /// <summary>
        /// Navigates to the next command in history or restores temporary input
        /// if at the end of history.
        /// </summary>
        /// <returns>The next command or the restored temporary input.</returns>
        public string Next()
        {
            if (_history.Count == 0) return _temporaryInput;

            _index = Mathf.Clamp(_index + 1, 0, _history.Count);

            if (_index == _history.Count)
            {
                var stored = _temporaryInput;
                ResetIndex();
                return stored; // restore unsaved input
            }

            return _history[_index];
        }

        /// <summary>
        /// Saves the current input text temporarily to restore it when navigating back.
        /// </summary>
        /// <param name="input">Current input to save.</param>
        public void SaveCurrentInput(string input)
        {
            if (!_savedTemporaryInput)
            {
                _temporaryInput = input;
                _savedTemporaryInput = true;
            }
        }

        /// <summary>
        /// Resets the history navigation index and clears temporary saved input.
        /// </summary>
        private void ResetIndex()
        {
            _index = _history.Count;
            _temporaryInput = string.Empty;
            _savedTemporaryInput = false;
        }
    }
}
