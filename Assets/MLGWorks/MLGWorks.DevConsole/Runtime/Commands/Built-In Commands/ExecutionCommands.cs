using MLGWorks.DevConsole.Runtime.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace MLGWorks.DevConsole.Runtime.Commands.BuiltIn
{
    /// <summary>
    /// Commands for invoking arbitrary methods via console.
    /// </summary>
    public static class ExecutionCommands
    {
        /// <summary>
        /// Invokes a static or instance method on a class with string arguments.
        /// Usage: invoke ClassName.MethodName arg1 arg2 ...
        /// </summary>
        /// <param name="fullMethodName">Full method name including class (e.g. "MyNamespace.MyClass.MyMethod")</param>
        /// <param name="args">Arguments as strings</param>
        /// <returns>Result of method call or error message</returns>
        [Command("invoke", "Invokes a static or instance method on a class with string args", "exec")]
        public static string Invoke(string fullMethodName, string[] args = null)
        {
            args ??= Array.Empty<string>();

            if (!TryResolveTargetAndMethod(fullMethodName, out var target, out var matchedMethod))
                return $"Method '{fullMethodName}' not found.";

            var parameters = matchedMethod.GetParameters();

            if (args.Length < parameters.Length)
                return $"Not enough arguments for method '{matchedMethod.Name}'. Expected {parameters.Length}, got {args.Length}.";

            object[] convertedParameters = new object[parameters.Length];
            bool convertible = true;

            if (parameters.Length > 0 && parameters.Last().ParameterType == typeof(string))
            {
                for (int i = 0; i < parameters.Length - 1; i++)
                {
                    try
                    {
                        convertedParameters[i] = ReflectionUtils.ParseValue(parameters[i].ParameterType, new[] { args[i] });
                    }
                    catch
                    {
                        convertible = false;
                        break;
                    }
                }

                if (convertible)
                {
                    convertedParameters[^1] = string.Join(" ", args.Skip(parameters.Length - 1));
                }
            }
            else
            {
                if (parameters.Length != args.Length)
                    return $"Method '{matchedMethod.Name}' expects {parameters.Length} arguments, but got {args.Length}.";

                for (int i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        convertedParameters[i] = ReflectionUtils.ParseValue(parameters[i].ParameterType, new[] { args[i] });
                    }
                    catch
                    {
                        convertible = false;
                        break;
                    }
                }
            }

            if (!convertible)
                return $"Failed to convert arguments for method '{matchedMethod.Name}'.";

            try
            {
                var result = matchedMethod.Invoke(target, convertedParameters);
                return result?.ToString() ?? "Method invoked successfully (void or null return).";
            }
            catch (TargetInvocationException tie)
            {
                return $"Exception thrown by method: {tie.InnerException?.Message ?? tie.Message}";
            }
            catch (Exception ex)
            {
                return $"Error invoking method: {ex.Message}";
            }
        }

        private static object GetSingletonInstance(Type type)
        {
            PropertyInfo instanceProperty = null;
            Type current = type;

            while (current != null)
            {
                instanceProperty = current.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (instanceProperty != null)
                    break;

                current = current.BaseType;
            }

            if (instanceProperty != null)
            {
                return instanceProperty.GetValue(null);
            }

            return null;
        }

        public static bool TryResolveTargetAndMethod(string fullMethodName, out object targetInstance, out MethodInfo matchedMethod)
        {
            targetInstance = null;
            matchedMethod = null;

            var parts = fullMethodName.Split('.');
            if (parts.Length < 2)
                return false;

            // Try static type first
            for (int methodIndex = parts.Length - 1; methodIndex >= 1; methodIndex--)
            {
                string className = string.Join(".", parts.Take(methodIndex));
                string methodName = parts[methodIndex];

                var type = ReflectionUtils.FindType(className);
                if (type != null)
                {
                    // Try to find static method on type
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    if (methods.Length > 0)
                    {
                        targetInstance = null;
                        matchedMethod = methods.First();
                        return true;
                    }
                }
            }

            // Try instance traversal
            Type currentType = ReflectionUtils.FindType(parts[0]);
            if (currentType == null)
                return false;

            object currentInstance = TryGetSingletonInstance(currentType);
            if (currentInstance == null)
                return false;

            for (int i = 1; i < parts.Length - 1; i++)
            {
                string memberName = parts[i];
                var field = currentType.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var prop = currentType.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                object nextInstance = null;

                if (field != null)
                {
                    nextInstance = field.GetValue(currentInstance);
                    currentType = field.FieldType;
                }
                else if (prop != null)
                {
                    nextInstance = prop.GetValue(currentInstance);
                    currentType = prop.PropertyType;
                }
                else
                {
                    return false;
                }

                if (nextInstance == null)
                    return false;

                currentInstance = nextInstance;
            }

            // Final method name
            string finalMethodName = parts[^1];
            var instanceMethods = currentType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name.Equals(finalMethodName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (instanceMethods.Length == 0)
                return false;

            targetInstance = currentInstance;
            matchedMethod = instanceMethods.First();
            return true;
        }

        public static object TryGetSingletonInstance(Type type)
        {
            return type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                       ?.GetValue(null);
        }
    }
}
