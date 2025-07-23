using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MLGWorks.DevConsole.Runtime.Utils
{
    public static class ReflectionUtils
    {
        public static Type FindType(string className)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .OrderBy(a =>
                {
                    string name = a.GetName().Name;
                    return name.StartsWith("Unity") || name.StartsWith("UnityEngine") || name.StartsWith("UnityEditor") ? 1 : 0;
                });

            foreach (var assembly in assemblies)
            {
                var type = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == className || t.FullName == className);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static MemberInfo FindMember(Type type, string name, out object target)
        {
            target = null;

            BindingFlags staticFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            BindingFlags instanceFlags = BindingFlags.Public | BindingFlags.Instance;

            // First try to find a static field or property
            var member = (MemberInfo)type.GetField(name, staticFlags)
                      ?? type.GetProperty(name, staticFlags);
            if (member != null)
                return member;

            // Try to resolve Singleton-style instance (look for "Instance" property)
            PropertyInfo instanceProperty = null;
            Type current = type;
            while (current != null)
            {
                instanceProperty = current.GetProperty("Instance", staticFlags);
                if (instanceProperty != null)
                    break;

                current = current.BaseType;
            }

            if (instanceProperty != null && instanceProperty.CanRead)
            {
                target = instanceProperty.GetValue(null);
                if (target != null)
                {
                    var instanceType = target.GetType(); // Use actual runtime type
                    member = (MemberInfo)instanceType.GetField(name, instanceFlags)
                          ?? instanceType.GetProperty(name, instanceFlags);
                    return member;
                }
            }

            return null;
        }

        public static object ParseValue(Type targetType, string[] args)
        {
            // Simple case: single string value
            if (targetType == typeof(string))
                return string.Join(" ", args);

            // Enhanced boolean parsing
            if (targetType == typeof(bool))
            {
                string input = string.Join(" ", args);
                if (bool.TryParse(input, out var b))
                    return b;

                if (input == "1" || input.Equals("yes", StringComparison.OrdinalIgnoreCase))
                    return true;
                if (input == "0" || input.Equals("no", StringComparison.OrdinalIgnoreCase))
                    return false;

                throw new FormatException($"Cannot convert '{input}' to bool.");
            }

            // Enums
            if (targetType.IsEnum)
                return Enum.Parse(targetType, string.Join(" ", args), ignoreCase: true);

            // Numbers and primitives
            if (targetType.IsPrimitive || targetType == typeof(decimal))
                return Convert.ChangeType(string.Join(" ", args), targetType, CultureInfo.InvariantCulture);

            // Array
            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType();
                var array = Array.CreateInstance(elementType, args.Length);
                for (int i = 0; i < args.Length; i++)
                    array.SetValue(Convert.ChangeType(args[i], elementType, CultureInfo.InvariantCulture), i);
                return array;
            }

            // Dictionary (expects key=value format)
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = targetType.GetGenericArguments()[0];
                var valueType = targetType.GetGenericArguments()[1];
                var dict = (IDictionary)Activator.CreateInstance(targetType);

                foreach (var pair in args)
                {
                    var split = pair.Split('=');
                    if (split.Length != 2)
                        throw new FormatException($"Invalid dictionary entry: {pair}");

                    object key = Convert.ChangeType(split[0], keyType, CultureInfo.InvariantCulture);
                    object value = Convert.ChangeType(split[1], valueType, CultureInfo.InvariantCulture);

                    dict.Add(key, value);
                }

                return dict;
            }

            throw new NotSupportedException($"Unsupported type: {targetType}");
        }
    }
}
