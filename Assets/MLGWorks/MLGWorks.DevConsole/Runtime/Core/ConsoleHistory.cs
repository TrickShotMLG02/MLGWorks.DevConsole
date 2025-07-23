using System;
using System.Collections.Generic;
using UnityEngine;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Keeps a history of entered commands for navigation.
    /// </summary>
    public class ConsoleHistory
    {
        private readonly List<string> _history = new List<string>();
        private int _index = -1;

        private string _temporaryInput = string.Empty;
        private bool _savedTemporaryInput = false;

        public void Add(string command)
        {
            if (_history.Count == 0 || _history[_history.Count - 1] != command)
            {
                _history.Add(command);
                ResetIndex();
            }
            _index = _history.Count;
        }

        public string Previous(string currentInput)
        {
            if (_history.Count == 0) return string.Empty;

            // Save the current input before navigating
            SaveCurrentInput(currentInput);

            _index = Mathf.Clamp(_index - 1, 0, _history.Count - 1);
            return _history[_index];
        }

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

        public void SaveCurrentInput(string input)
        {
            if (!_savedTemporaryInput)
            {
                _temporaryInput = input;
                _savedTemporaryInput = true;
            }
        }

        private void ResetIndex()
        {
            _index = _history.Count;
            _temporaryInput = string.Empty;
            _savedTemporaryInput = false;
        }
    }
}
