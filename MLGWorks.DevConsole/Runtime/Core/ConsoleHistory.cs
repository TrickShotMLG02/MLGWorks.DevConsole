using System.Collections.Generic;
using UnityEngine;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Keeps a history of entered commands for navigation.
    /// </summary>
    public class ConsoleHistory
    {
        private readonly List<string> history = new List<string>();
        private int index = -1;

        public void Add(string command)
        {
            history.Add(command);
            index = history.Count;
        }

        public string Previous()
        {
            if (history.Count == 0) return string.Empty;
            index = Mathf.Clamp(index - 1, 0, history.Count - 1);
            return history[index];
        }

        public string Next()
        {
            if (history.Count == 0) return string.Empty;
            index = Mathf.Clamp(index + 1, 0, history.Count);
            return index < history.Count ? history[index] : string.Empty;
        }
    }
}
