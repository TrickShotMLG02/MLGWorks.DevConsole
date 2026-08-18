using System;
using System.Collections.Generic;
using MLGWorks.DevConsole.Runtime.Utils;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.CoreTests
{
    public class ReflectionUtilsTests
    {
        public enum TestMode { Alpha, Beta }

        [TestCase("true", true)]
        [TestCase("TRUE", true)]
        [TestCase("yes", true)]
        [TestCase("1", true)]
        [TestCase("false", false)]
        [TestCase("no", false)]
        [TestCase("0", false)]
        public void ParseValueSupportsBooleanForms(string input, bool expected)
        {
            Assert.That(ReflectionUtils.ParseValue(typeof(bool), new[] { input }), Is.EqualTo(expected));
        }

        [TestCase("Alpha", TestMode.Alpha)]
        [TestCase("beta", TestMode.Beta)]
        public void ParseValueParsesEnumsCaseInsensitively(string input, TestMode expected)
        {
            Assert.That(ReflectionUtils.ParseValue(typeof(TestMode), new[] { input }), Is.EqualTo(expected));
        }

        [TestCase(typeof(int), "42", 42)]
        [TestCase(typeof(float), "1.5", 1.5f)]
        [TestCase(typeof(double), "2.5", 2.5d)]
        [TestCase(typeof(decimal), "3.5", 3.5d)]
        public void ParseValueUsesInvariantNumericCulture(Type type, string input, double expected)
        {
            Assert.That(Convert.ToDouble(ReflectionUtils.ParseValue(type, new[] { input })), Is.EqualTo(expected).Within(0.0001));
        }

        [Test]
        public void ParseValueJoinsStringArguments()
        {
            Assert.That(ReflectionUtils.ParseValue(typeof(string), new[] { "hello", "world" }), Is.EqualTo("hello world"));
        }

        [Test]
        public void ParseValueParsesArrays()
        {
            var result = (int[])ReflectionUtils.ParseValue(typeof(int[]), new[] { "1", "2", "3" });
            Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void ParseValueParsesDictionaries()
        {
            var result = (Dictionary<string, int>)ReflectionUtils.ParseValue(
                typeof(Dictionary<string, int>), new[] { "one=1", "two=2" });
            Assert.That(result["one"], Is.EqualTo(1));
            Assert.That(result["two"], Is.EqualTo(2));
        }

        [TestCase("maybe")]
        [TestCase("2")]
        public void ParseValueRejectsInvalidBooleans(string input)
        {
            Assert.Throws<FormatException>(() => ReflectionUtils.ParseValue(typeof(bool), new[] { input }));
        }

        [Test]
        public void ParseValueRejectsMalformedDictionaryEntries()
        {
            Assert.Throws<FormatException>(() => ReflectionUtils.ParseValue(
                typeof(Dictionary<string, int>), new[] { "invalid" }));
        }

        [Test]
        public void ParseValueRejectsUnsupportedTypes()
        {
            Assert.Throws<NotSupportedException>(() => ReflectionUtils.ParseValue(typeof(Uri), new[] { "x" }));
        }

        [Test]
        public void FindTypeFindsSimpleAndQualifiedNames()
        {
            Assert.That(ReflectionUtils.FindType(nameof(ReflectionUtilsTests)), Is.EqualTo(typeof(ReflectionUtilsTests)));
            Assert.That(ReflectionUtils.FindType(typeof(ReflectionUtilsTests).FullName), Is.EqualTo(typeof(ReflectionUtilsTests)));
        }

        [Test]
        public void FindTypeReturnsNullForMissingType()
        {
            Assert.That(ReflectionUtils.FindType("DefinitelyMissingType_DevConsole"), Is.Null);
        }
    }
}
