using System.Collections.Generic;

namespace MLGWorks.DevConsole.Runtime.UI
{
    /// <summary>
    /// Maintains a scrollback buffer to limit displayed lines.
    /// </summary>
    public class ScrollbackBuffer
    {
        private readonly Queue<string> lines = new Queue<string>();
        private readonly int maxLines;

        public ScrollbackBuffer(int maxLines)
        {
            this.maxLines = maxLines;
        }

        public void Add(string line)
        {
            lines.Enqueue(line);
            if (lines.Count > maxLines)
                lines.Dequeue();
        }

        public IEnumerable<string> GetLines() => lines;
    }
}
