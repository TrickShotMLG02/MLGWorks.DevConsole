using System;
using System.Collections.Generic;

namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Abstracts console input actions from Unity's generated Input System wrapper.
    /// </summary>
    public interface IConsoleInput : IDisposable
    {
        event Action ToggleConsole;
        event Action SubmitCommand;
        event Action AutoComplete;
        event Action HistoryPrevious;
        event Action HistoryNext;

        IReadOnlyList<string> HistoryBindingPaths { get; }

        void Enable();
        void Disable();
    }
}
