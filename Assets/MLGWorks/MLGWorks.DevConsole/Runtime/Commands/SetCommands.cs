using MLGWorks.DevConsole.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MLGWorks.DevConsole.Runtime.Commands
{
    public static class SetCommand
    {
        [Command("Set", "Sets the value of a variable of a given class to a specific value")]
        public static string Set(string className, string variableName, string[] args)
        {
            if (args.Length < 1)
            {
                return $"Usage: set<ClassName> < VariableName > < Value >";
            }

            Type type = FindType(className);
            if (type == null)
            {
                return $"Type '{className}' not found.";
            }

            object target = null;
            var member = FindMember(type, variableName, out target);

            if (member == null)
            {
                return $"Variable '{variableName}' not found in {className}.";
            }

            Type valueType = member is FieldInfo fi ? fi.FieldType : ((PropertyInfo)member).PropertyType;

            try
            {
                object parsedValue = ParseValue(valueType, args);

                object oldValue = null;

                if (member is FieldInfo field)
                {
                    oldValue = field.GetValue(target);
                    field.SetValue(target, parsedValue);
                }
                else if (member is PropertyInfo property && property.CanWrite)
                {
                    oldValue = property.GetValue(target);
                    property.SetValue(target, parsedValue);
                }

                return $"Set {className}.{variableName} {oldValue} => {parsedValue}";
            }
            catch (Exception ex)
            {
                return $"Failed to set value: {ex.Message}";
            }
        }

        private static object ParseValue(Type targetType, string[] args)
        {
            // Simple case: single string value
            if (targetType == typeof(string))
                return string.Join(" ", args);

            // Simple types
            if (targetType.IsPrimitive || targetType.IsEnum || targetType == typeof(decimal) || targetType == typeof(bool))
                return Convert.ChangeType(string.Join(" ", args), targetType);

            // Array
            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType();
                var array = Array.CreateInstance(elementType, args.Length);
                for (int i = 0; i < args.Length; i++)
                    array.SetValue(Convert.ChangeType(args[i], elementType), i);
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

                    object key = Convert.ChangeType(split[0], keyType);
                    object value = Convert.ChangeType(split[1], valueType);

                    dict.Add(key, value);
                }

                return dict;
            }

            throw new NotSupportedException($"Unsupported type: {targetType}");
        }

        private static Type FindType(string className)
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

        private static MemberInfo FindMember(Type type, string name, out object target)
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
    }
}
