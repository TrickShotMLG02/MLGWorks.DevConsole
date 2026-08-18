using MLGWorks.DevConsole.Runtime.Core;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.CoreTests
{
    public class ConsoleHistoryTests
    {
        [Test]
        public void EmptyHistoryReturnsEmptyValues()
        {
            var history = new ConsoleHistory();

            Assert.That(history.Previous("draft"), Is.Empty);
            Assert.That(history.Next(), Is.Empty);
        }

        [Test]
        public void PreviousClampsAtOldestCommand()
        {
            var history = new ConsoleHistory();
            history.Add("one");
            history.Add("two");

            history.Previous(string.Empty);
            Assert.That(history.Previous("two"), Is.EqualTo("one"));
            Assert.That(history.Previous("one"), Is.EqualTo("one"));
        }

        [Test]
        public void NextClampsAtNewestCommandAndRestoresDraft()
        {
            var history = new ConsoleHistory();
            history.Add("one");

            Assert.That(history.Previous("draft"), Is.EqualTo("one"));
            Assert.That(history.Next(), Is.EqualTo("draft"));
            Assert.That(history.Next(), Is.Empty);
        }

        [Test]
        public void NonConsecutiveDuplicatesAreRetained()
        {
            var history = new ConsoleHistory();
            history.Add("one");
            history.Add("two");
            history.Add("one");

            Assert.That(history.Previous(string.Empty), Is.EqualTo("one"));
            Assert.That(history.Previous("one"), Is.EqualTo("two"));
        }

        [Test]
        public void AddingCommandResetsDraftNavigation()
        {
            var history = new ConsoleHistory();
            history.Add("one");
            history.Previous("draft");
            history.Add("two");

            Assert.That(history.Previous(string.Empty), Is.EqualTo("two"));
            Assert.That(history.Next(), Is.Empty);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void HistorySupportsRepeatedNavigation(int navigationCount)
        {
            var history = new ConsoleHistory();
            history.Add("one");
            history.Add("two");

            for (var i = 0; i < navigationCount; i++)
                history.Previous(string.Empty);

            Assert.That(history.Next(), Is.Not.Null);
        }

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
