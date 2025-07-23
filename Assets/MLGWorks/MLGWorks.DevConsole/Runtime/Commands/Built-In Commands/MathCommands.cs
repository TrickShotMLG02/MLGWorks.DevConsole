using System;

namespace MLGWorks.DevConsole.Runtime.Commands.BuiltIn
{
    public static class MathCommands
    {
        [Command("add", "Adds two numbers")]
        public static string Add(float a, float b) => (a + b).ToString();

        [Command("sub", "Subtracts the second number from the first")]
        public static string Subtract(float a, float b) => (a - b).ToString();

        [Command("mul", "Multiplies two numbers")]
        public static string Multiply(float a, float b) => (a * b).ToString();

        [Command("div", "Divides the first number by the second")]
        public static string Divide(float a, float b)
        {
            if (b == 0f)
                return "Error: Division by zero.";
            return (a / b).ToString();
        }

        [Command("mod", "Returns the remainder of a divided by b")]
        public static string Modulo(float a, float b)
        {
            if (b == 0f)
                return "Error: Modulo by zero.";
            return (a % b).ToString();
        }

        [Command("pow", "Raises a to the power of b")]
        public static string Power(float a, float b) => Math.Pow(a, b).ToString();

        [Command("sqrt", "Returns the square root of a number")]
        public static string Sqrt(float a)
        {
            if (a < 0f)
                return "Error: Square root of negative number.";
            return Math.Sqrt(a).ToString();
        }

        [Command("abs", "Returns the absolute value of a number")]
        public static string Abs(float a) => Math.Abs(a).ToString();

        [Command("min", "Returns the smaller of two numbers")]
        public static string Min(float a, float b) => Math.Min(a, b).ToString();

        [Command("max", "Returns the larger of two numbers")]
        public static string Max(float a, float b) => Math.Max(a, b).ToString();

        [Command("round", "Rounds to the nearest integer")]
        public static string Round(float a) => Math.Round(a).ToString();

        [Command("floor", "Rounds down to the nearest whole number")]
        public static string Floor(float a) => Math.Floor(a).ToString();

        [Command("ceil", "Rounds up to the nearest whole number")]
        public static string Ceil(float a) => Math.Ceiling(a).ToString();

        [Command("sign", "Returns the sign of a number (-1, 0, 1)")]
        public static string Sign(float a) => Math.Sign(a).ToString();
    }
}
