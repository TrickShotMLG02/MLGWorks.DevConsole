using System.Linq;
using System;
using MLGWorks.DevConsole.Runtime.UI;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.CoreTests
{
    public class ScrollbackBufferTests
    {
        [TestCase(1, "one", new[] { "one" })]
        [TestCase(2, "one", new[] { "one" })]
        [TestCase(3, "one\ntwo\nthree", new[] { "one", "two", "three" })]
        public void BufferHonorsCapacity(int capacity, string input, string[] expected)
        {
            var buffer = new ScrollbackBuffer(capacity);
            buffer.Add(input);

            Assert.That(buffer.GetLines().ToArray(), Is.EqualTo(expected));
        }

        [Test]
        public void BufferHandlesAllSupportedLineEndings()
        {
            var buffer = new ScrollbackBuffer(5);
            buffer.Add("one\rtwo\r\nthree\nfour");

            Assert.That(buffer.GetLines().ToArray(), Is.EqualTo(new[] { "one", "two", "three", "four" }));
        }

        [Test]
        public void BufferKeepsNewestLinesAcrossMultipleAdds()
        {
            var buffer = new ScrollbackBuffer(3);
            buffer.Add("one\ntwo");
            buffer.Add("three\nfour");
            buffer.Add("five");

            Assert.That(buffer.GetLines().ToArray(), Is.EqualTo(new[] { "three", "four", "five" }));
        }

        [Test]
        public void BufferRetainsEmptyLines()
        {
            var buffer = new ScrollbackBuffer(3);
            buffer.Add("one\n\nthree");

            Assert.That(buffer.GetLines().ToArray(), Is.EqualTo(new[] { "one", "", "three" }));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void BufferRejectsNonPositiveCapacity(int capacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScrollbackBuffer(capacity));
        }

        [Test]
        public void BufferRejectsNullLines()
        {
            var buffer = new ScrollbackBuffer(1);
            Assert.Throws<ArgumentNullException>(() => buffer.Add(null));
        }
        [Test]
        public void BufferRetainsOnlyTheNewestLines()
        {
            var buffer = new ScrollbackBuffer(2);
            buffer.Add("one\ntwo");
            buffer.Add("three");

            Assert.That(buffer.GetLines().ToArray(), Is.EqualTo(new[] { "two", "three" }));
        }
    }
}
