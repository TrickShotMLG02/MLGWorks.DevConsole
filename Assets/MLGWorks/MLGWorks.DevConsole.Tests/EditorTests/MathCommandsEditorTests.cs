using System;
using System.Globalization;
using MLGWorks.DevConsole.Runtime.Commands.BuiltIn;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.EditorTests
{
    public class MathCommandsEditorTests
    {
        [TestCase(0f, 0f, 0f)]
        [TestCase(1.5f, 2.5f, 4f)]
        [TestCase(-1.5f, 2.5f, 1f)]
        [TestCase(-10f, -5f, -15f)]
        [TestCase(float.MaxValue, float.MaxValue, float.PositiveInfinity)]
        public void AddCoversSignsZeroAndOverflow(float a, float b, float expected)
        {
            AssertFloat(MathCommands.Add(a, b), expected);
        }

        [TestCase(0f, 0f, 0f)]
        [TestCase(5.5f, 2.5f, 3f)]
        [TestCase(-5.5f, 2.5f, -8f)]
        [TestCase(-5.5f, -2.5f, -3f)]
        [TestCase(float.NegativeInfinity, float.PositiveInfinity, float.NegativeInfinity)]
        public void SubtractCoversSignsAndInfinity(float a, float b, float expected)
        {
            AssertFloat(MathCommands.Subtract(a, b), expected);
        }

        [TestCase(0f, 100f, 0f)]
        [TestCase(-2f, 3f, -6f)]
        [TestCase(-2f, -3f, 6f)]
        [TestCase(float.MaxValue, 2f, float.PositiveInfinity)]
        [TestCase(float.PositiveInfinity, 0f, float.NaN)]
        public void MultiplyCoversSignsZeroAndOverflow(float a, float b, float expected)
        {
            AssertFloat(MathCommands.Multiply(a, b), expected);
        }

        [TestCase(10f, 2f, 5f)]
        [TestCase(-10f, 2f, -5f)]
        [TestCase(-10f, -2f, 5f)]
        [TestCase(0f, 2f, 0f)]
        public void DivideCoversSignsAndZeroNumerator(float a, float b, float expected)
        {
            AssertFloat(MathCommands.Divide(a, b), expected);
        }

        [TestCase(1f, 0f)]
        [TestCase(1f, -0f)]
        [TestCase(-1f, 0f)]
        public void DivideRejectsBothSignsOfZeroDenominator(float a, float b)
        {
            Assert.That(MathCommands.Divide(a, b), Is.EqualTo("Error: Division by zero."));
        }

        [Test]
        public void DividePreservesInfinityForNonzeroFiniteDenominator()
        {
            AssertFloat(MathCommands.Divide(float.MaxValue, float.Epsilon), float.PositiveInfinity);
        }

        [TestCase(10f, 3f, 1f)]
        [TestCase(-10f, 3f, -1f)]
        [TestCase(10f, -3f, 1f)]
        [TestCase(0f, 3f, 0f)]
        public void ModuloCoversSignedOperands(float a, float b, float expected)
        {
            AssertFloat(MathCommands.Modulo(a, b), expected);
        }

        [TestCase(10f, 0f)]
        [TestCase(10f, -0f)]
        public void ModuloRejectsBothSignsOfZeroDenominator(float a, float b)
        {
            Assert.That(MathCommands.Modulo(a, b), Is.EqualTo("Error: Modulo by zero."));
        }

        [TestCase(2f, 0f, 1d)]
        [TestCase(2f, 3f, 8d)]
        [TestCase(2f, -2f, 0.25d)]
        [TestCase(-2f, 3f, -8d)]
        [TestCase(0f, 0f, 1d)]
        public void PowerCoversZeroNegativeAndFractionalExponents(float a, float b, double expected)
        {
            AssertDouble(MathCommands.Power(a, b), expected);
        }

        [Test]
        public void PowerUsesDoublePrecisionForLargeFiniteResults()
        {
            AssertDouble(MathCommands.Power(float.MaxValue, 2f), Math.Pow(float.MaxValue, 2d));
        }

        [TestCase(0f, 0f)]
        [TestCase(1f, 1f)]
        [TestCase(4f, 2f)]
        [TestCase(2.25f, 1.5f)]
        public void SqrtCoversZeroPerfectAndFractionalSquares(float input, float expected)
        {
            AssertDouble(MathCommands.Sqrt(input), expected);
        }

        [TestCase(-1f)]
        [TestCase(float.NegativeInfinity)]
        public void SqrtRejectsNegativeInputs(float input)
        {
            Assert.That(MathCommands.Sqrt(input), Is.EqualTo("Error: Square root of negative number."));
        }

        [TestCase(-10f, 10f)]
        [TestCase(10f, 10f)]
        [TestCase(0f, 0f)]
        public void AbsReturnsNonnegativeMagnitude(float input, float expected)
        {
            AssertFloat(MathCommands.Abs(input), expected);
        }

        [Test]
        public void AbsHandlesMaximumFiniteFloat()
        {
            AssertFloat(MathCommands.Abs(float.MinValue), float.MaxValue);
        }

        [TestCase(-2f, 3f, -2f)]
        [TestCase(3f, -2f, -2f)]
        [TestCase(2f, 2f, 2f)]
        [TestCase(float.NaN, 2f, float.NaN)]
        public void MinCoversOrderingEqualityAndNaN(float a, float b, float expected)
        {
            AssertFloat(MathCommands.Min(a, b), expected);
        }

        [TestCase(-2f, 3f, 3f)]
        [TestCase(3f, -2f, 3f)]
        [TestCase(2f, 2f, 2f)]
        [TestCase(float.NaN, 2f, float.NaN)]
        public void MaxCoversOrderingEqualityAndNaN(float a, float b, float expected)
        {
            AssertFloat(MathCommands.Max(a, b), expected);
        }

        [TestCase(2.4f, 2d)]
        [TestCase(2.5f, 2d)]
        [TestCase(3.5f, 4d)]
        [TestCase(-2.5f, -2d)]
        [TestCase(-3.5f, -4d)]
        public void RoundUsesBankersRounding(float input, double expected)
        {
            AssertDouble(MathCommands.Round(input), expected);
        }

        [TestCase(2.9f, 2d)]
        [TestCase(-2.1f, -3d)]
        [TestCase(-2.0f, -2d)]
        [TestCase(0f, 0d)]
        public void FloorRoundsTowardNegativeInfinity(float input, double expected)
        {
            AssertDouble(MathCommands.Floor(input), expected);
        }

        [TestCase(2.1f, 3d)]
        [TestCase(-2.9f, -2d)]
        [TestCase(-2.0f, -2d)]
        [TestCase(0f, 0d)]
        public void CeilRoundsTowardPositiveInfinity(float input, double expected)
        {
            AssertDouble(MathCommands.Ceil(input), expected);
        }

        [TestCase(-10f, -1)]
        [TestCase(-float.Epsilon, -1)]
        [TestCase(0f, 0)]
        [TestCase(float.Epsilon, 1)]
        [TestCase(10f, 1)]
        public void SignReturnsExpectedSign(float input, int expected)
        {
            Assert.That(MathCommands.Sign(input), Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void SignRejectsNaN()
        {
            Assert.Throws<ArithmeticException>(() => MathCommands.Sign(float.NaN));
        }

        private static void AssertFloat(string actual, float expected)
        {
            var parsed = float.Parse(actual, CultureInfo.CurrentCulture);
            if (float.IsNaN(expected))
                Assert.That(float.IsNaN(parsed), Is.True);
            else if (float.IsInfinity(expected))
                Assert.That(parsed, Is.EqualTo(expected));
            else
                Assert.That(parsed, Is.EqualTo(expected).Within(Math.Max(0.00001f, Math.Abs(expected) * 0.00001f)));
        }

        private static void AssertDouble(string actual, double expected)
        {
            var parsed = double.Parse(actual, CultureInfo.CurrentCulture);
            if (double.IsInfinity(expected))
                Assert.That(parsed, Is.EqualTo(expected));
            else
                Assert.That(parsed, Is.EqualTo(expected).Within(Math.Max(0.0000001d, Math.Abs(expected) * 0.0000001d)));
        }
    }
}
