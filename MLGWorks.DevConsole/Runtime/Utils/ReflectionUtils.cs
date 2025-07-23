using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MLGWorks.DevConsole.Runtime.Utils
{
    /// <summary>
    /// Utility class providing reflection-based methods to find types, members, and parse string arguments into typed values.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Finds a <see cref="Type"/> by its name or fully qualified name by searching all loaded assemblies.
        /// Assemblies with names starting with "Unity", "UnityEngine", or "UnityEditor" are searched last.
        /// </summary>
        /// <param name="className">The simple or fully qualified name of the class to find.</param>
        /// <returns>The <see cref="Type"/> if found; otherwise, <c>null</c>.</returns>
        public static Type FindType(string className)
        {
            // Sort assemblies to put Unity-related assemblies last
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

        /// <summary>
        /// Finds a member (field or property) of a given type by name.
        /// Tries static members first, then attempts to resolve a singleton instance by looking for an "Instance" property.
        /// If an instance member is found, <paramref name="target"/> will be set to the instance object.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to search in.</param>
        /// <param name="name">The name of the member to find.</param>
        /// <param name="target">
        /// Out parameter that will be set to the instance owning the member if it is an instance member; otherwise, <c>null</c>.
        /// </param>
        /// <returns>The <see cref="MemberInfo"/> if found; otherwise, <c>null</c>.</returns>
        public static MemberInfo FindMember(Type type, string name, out object target)
        {
            target = null;

            BindingFlags staticFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            BindingFlags instanceFlags = BindingFlags.Public | BindingFlags.Instance;

            // Try to find static field or property
            var member = (MemberInfo)type.GetField(name, staticFlags)
                      ?? type.GetProperty(name, staticFlags);
            if (member != null)
                return member;

            // Attempt to find a singleton instance by looking for "Instance" property up the inheritance chain
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
                    var instanceType = target.GetType(); // Use the runtime type of the instance
                    member = (MemberInfo)instanceType.GetField(name, instanceFlags)
                          ?? instanceType.GetProperty(name, instanceFlags);
                    return member;
                }
            }

            return null;
        }

        /// <summary>
        /// Parses an array of string arguments into an object of the specified target type.
        /// Supports strings, booleans (with extended parsing), enums, primitives, arrays, and dictionaries.
        /// </summary>
        /// <param name="targetType">The target <see cref="Type"/> to parse into.</param>
        /// <param name="args">The string arguments to parse.</param>
        /// <returns>An object of the specified type parsed from the string arguments.</returns>
        /// <exception cref="FormatException">Thrown when parsing fails due to invalid input format.</exception>
        /// <exception cref="NotSupportedException">Thrown when the target type is not supported.</exception>
        public static object ParseValue(Type targetType, string[] args)
        {
            // Simple case: single string value
            if (targetType == typeof(string))
                return string.Join(" ", args);

            // Enhanced boolean parsing with support for common variations
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

            // Enum parsing (case insensitive)
            if (targetType.IsEnum)
                return Enum.Parse(targetType, string.Join(" ", args), ignoreCase: true);

            // Primitives and decimal parsing using invariant culture
            if (targetType.IsPrimitive || targetType == typeof(decimal))
                return Convert.ChangeType(string.Join(" ", args), targetType, CultureInfo.InvariantCulture);

            // Array parsing: convert each string argument to the element type
            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType();
                var array = Array.CreateInstance(elementType, args.Length);
                for (int i = 0; i < args.Length; i++)
                    array.SetValue(Convert.ChangeType(args[i], elementType, CultureInfo.InvariantCulture), i);
                return array;
            }

            // Dictionary parsing: expects each arg in "key=value" format
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

        /// <summary>
        /// Gets all loaded assemblies sorted so that:
        /// - Non-Unity and non-MLGWorks assemblies come first,
        /// - MLGWorks assemblies come second,
        /// - Unity assemblies come last.
        /// </summary>
        /// <returns>Sorted enumerable of assemblies.</returns>
        public static IOrderedEnumerable<Assembly> GetSortedAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .OrderBy(a =>
                {
                    string name = a.GetName().Name;

                    // Prioritize assemblies as per custom logic
                    if (name.StartsWith("Unity") || name.StartsWith("UnityEngine") || name.StartsWith("UnityEditor"))
                        return 20;   // last group
                    else if (name.StartsWith("MLGWorks"))
                        return 19;   // second last group
                    else
                        return 0;    // first group
                });

            return assemblies;
        }
    }
}
