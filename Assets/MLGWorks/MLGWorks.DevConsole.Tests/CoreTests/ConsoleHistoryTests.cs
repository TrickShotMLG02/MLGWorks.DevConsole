using MLGWorks.DevConsole.Runtime.Core;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.CoreTests
{
    public class ConsoleHistoryTests
    {
        [Test]
        public void PreviousAndNextRestoreTemporaryInput()
        {
            var history = new ConsoleHistory();
            history.Add("first");
            history.Add("second");

            Assert.That(history.Previous("draft"), Is.EqualTo("second"));
            Assert.That(history.Previous("second"), Is.EqualTo("first"));
            Assert.That(history.Next(), Is.EqualTo("second"));
            Assert.That(history.Next(), Is.EqualTo("draft"));
        }

        [Test]
        public void AddDoesNotRepeatTheLastCommand()
        {
            var history = new ConsoleHistory();
            history.Add("same");
            history.Add("same");

            Assert.That(history.Previous(string.Empty), Is.EqualTo("same"));
            Assert.That(history.Previous("same"), Is.EqualTo("same"));
        }
    }
}
