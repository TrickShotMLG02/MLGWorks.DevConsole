using System.Linq;
using MLGWorks.DevConsole.Runtime.UI;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.CoreTests
{
    public class ScrollbackBufferTests
    {
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
