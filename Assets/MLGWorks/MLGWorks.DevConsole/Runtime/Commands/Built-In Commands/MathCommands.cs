using System;

namespace MLGWorks.DevConsole.Runtime.Commands.BuiltIn
{
    /// <summary>
    /// Provides basic mathematical operations as console commands.
    /// </summary>
    public static class MathCommands
    {
        /// <summary>
        /// Adds two numbers.
        /// </summary>
        [Command("add", "Adds two numbers")]
        public static string Add(float a, float b) => (a + b).ToString();

        /// <summary>
        /// Subtracts the second number from the first.
        /// </summary>
        [Command("sub", "Subtracts the second number from the first")]
        public static string Subtract(float a, float b) => (a - b).ToString();

        /// <summary>
        /// Multiplies two numbers.
        /// </summary>
        [Command("mul", "Multiplies two numbers")]
        public static string Multiply(float a, float b) => (a * b).ToString();

        /// <summary>
        /// Divides the first number by the second.
        /// Returns an error if dividing by zero.
        /// </summary>
        [Command("div", "Divides the first number by the second")]
        public static string Divide(float a, float b)
        {
            if (b == 0f)
                return "Error: Division by zero.";
            return (a / b).ToString();
        }

        /// <summary>
        /// Returns the remainder of a divided by b.
        /// Returns an error if modulo by zero.
        /// </summary>
        [Command("mod", "Returns the remainder of a divided by b")]
        public static string Modulo(float a, float b)
        {
            if (b == 0f)
                return "Error: Modulo by zero.";
            return (a % b).ToString();
        }

        /// <summary>
        /// Raises a to the power of b.
        /// </summary>
        [Command("pow", "Raises a to the power of b")]
        public static string Power(float a, float b) => Math.Pow(a, b).ToString();

        /// <summary>
        /// Returns the square root of a number.
        /// Returns an error if the number is negative.
        /// </summary>
        [Command("sqrt", "Returns the square root of a number")]
        public static string Sqrt(float a)
        {
            if (a < 0f)
                return "Error: Square root of negative number.";
            return Math.Sqrt(a).ToString();
        }

        /// <summary>
        /// Returns the absolute value of a number.
        /// </summary>
        [Command("abs", "Returns the absolute value of a number")]
        public static string Abs(float a) => Math.Abs(a).ToString();

        /// <summary>
        /// Returns the smaller of two numbers.
        /// </summary>
        [Command("min", "Returns the smaller of two numbers")]
        public static string Min(float a, float b) => Math.Min(a, b).ToString();

        /// <summary>
        /// Returns the larger of two numbers.
        /// </summary>
        [Command("max", "Returns the larger of two numbers")]
        public static string Max(float a, float b) => Math.Max(a, b).ToString();

        /// <summary>
        /// Rounds a number to the nearest integer.
        /// </summary>
        [Command("round", "Rounds to the nearest integer")]
        public static string Round(float a) => Math.Round(a).ToString();

        /// <summary>
        /// Rounds a number down to the nearest whole number.
        /// </summary>
        [Command("floor", "Rounds down to the nearest whole number")]
        public static string Floor(float a) => Math.Floor(a).ToString();

        /// <summary>
        /// Rounds a number up to the nearest whole number.
        /// </summary>
        [Command("ceil", "Rounds up to the nearest whole number")]
        public static string Ceil(float a) => Math.Ceiling(a).ToString();

        /// <summary>
        /// Returns the sign of a number: -1 if negative, 0 if zero, 1 if positive.
        /// </summary>
        [Command("sign", "Returns the sign of a number (-1, 0, 1)")]
        public static string Sign(float a) => Math.Sign(a).ToString();
    }
}
