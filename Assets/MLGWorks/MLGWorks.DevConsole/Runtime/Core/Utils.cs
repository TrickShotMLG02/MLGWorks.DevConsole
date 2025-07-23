using System;

namespace MLGWorks.DevConsole.Runtime.Core
{
    /// <summary>
    /// Utility methods for common operations.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Converts a <see cref="Type"/> to a human-readable type name string,
        /// using C# aliases for primitive types and properly formatting arrays and enums.
        /// </summary>
        /// <param name="type">The type to get a readable name for.</param>
        /// <returns>A readable string representation of the type.</returns>
        public static string GetReadableTypeName(Type type)
        {
            if (type == typeof(int)) return "int";
            if (type == typeof(string)) return "string";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type.IsEnum) return type.Name;
            if (type.IsArray) return $"{GetReadableTypeName(type.GetElementType())}[]";
            return type.Name;
        }
    }
}
