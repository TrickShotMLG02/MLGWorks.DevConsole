using System;
using System.Collections.Generic;
using MLGWorks.DevConsole.Runtime.Abstractions;

namespace MLGWorks.DevConsole.Runtime.UI
{
    /// <summary>
    /// Maintains a scrollback buffer that holds a limited number of lines.
    /// Automatically removes the oldest lines when the buffer exceeds the maximum size.
    /// </summary>
    public class ScrollbackBuffer : IScrollbackBuffer
    {
        private readonly Queue<string> lines = new Queue<string>();
        private readonly int maxLines;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollbackBuffer"/> class.
        /// </summary>
        /// <param name="maxLines">The maximum number of lines to retain in the buffer.</param>
        public ScrollbackBuffer(int maxLines)
        {
            if (maxLines <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLines), "The maximum line count must be greater than zero.");

            this.maxLines = maxLines;
        }

        /// <summary>
        /// Adds a new line to the buffer. If the buffer exceeds the maximum number of lines,
        /// the oldest line is removed.
        /// </summary>
        /// <param name="line">The line to add.</param>
        public void Add(string line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            // Split on all newlines (\r\n, \n, \r)
            var splitLines = line.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            foreach (var singleLine in splitLines)
            {
                lines.Enqueue(singleLine);
                if (lines.Count > maxLines)
                    lines.Dequeue();
            }
        }

        /// <summary>
        /// Gets an enumerable collection of all lines currently in the buffer,
        /// in the order they were added (oldest first).
        /// </summary>
        /// <returns>Enumerable of lines in the buffer.</returns>
        public IEnumerable<string> GetLines() => lines;
    }
}
