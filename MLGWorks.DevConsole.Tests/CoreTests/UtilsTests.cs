using System;
using MLGWorks.DevConsole.Runtime.Core;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.CoreTests
{
    public class UtilsTests
    {
        [TestCase(typeof(int), "int")]
        [TestCase(typeof(string), "string")]
        [TestCase(typeof(bool), "bool")]
        [TestCase(typeof(float), "float")]
        [TestCase(typeof(double), "double")]
        [TestCase(typeof(TestEnum), "TestEnum")]
        [TestCase(typeof(int[]), "int[]")]
        [TestCase(typeof(string[]), "string[]")]
        [TestCase(typeof(TestEnum[]), "TestEnum[]")]
        [TestCase(typeof(DateTime), "DateTime")]
        public void GetReadableTypeNameReturnsStableNames(Type type, string expected)
        {
            Assert.That(MLGWorks.DevConsole.Runtime.Core.Utils.GetReadableTypeName(type), Is.EqualTo(expected));
        }

        private enum TestEnum
        {
            First,
            Second
        }
    }
}
