using System;

namespace MLGWorks.DevConsole.Runtime.Core
{
    public static class Utils
    {
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
