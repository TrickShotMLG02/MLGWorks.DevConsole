using System.Collections.Generic;

namespace MLGWorks.DevConsole.Runtime.Abstractions
{
    /// <summary>
    /// Stores a bounded sequence of console lines.
    /// </summary>
    public interface IScrollbackBuffer
    {
        void Add(string line);
        IEnumerable<string> GetLines();
    }
}
