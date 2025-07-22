using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.UI;
using UnityEngine;
using MLGWorks.Utils.Logging;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Captures and processes input from the console input field.
    /// </summary>
    [DisallowMultipleComponent]
    public class InputHandler : MonoBehaviour
    {
        public void SubmitCommand(string input)
        {
            string result = null;
            CommandManager.TryExecute(input, out result);

            if (result != null)
                ConsoleUI.Instance.AppendToOutput(result, LogLevel.Output);
        }
    }
}
