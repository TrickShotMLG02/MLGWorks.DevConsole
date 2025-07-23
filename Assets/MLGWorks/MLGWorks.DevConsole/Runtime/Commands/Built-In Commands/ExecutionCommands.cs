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

            // Split fullMethodName into className and methodName by last '.'
            int lastDotIndex = fullMethodName.LastIndexOf('.');
            if (lastDotIndex <= 0 || lastDotIndex == fullMethodName.Length - 1)
                return "Invalid method format. Use ClassName.MethodName";

            string className = fullMethodName.Substring(0, lastDotIndex);
            string methodName = fullMethodName.Substring(lastDotIndex + 1);

            Type type = ReflectionUtils.FindType(className);
            if (type == null)
                return $"Type '{className}' not found.";

            object target = null;

            // Find all methods matching the name (case-insensitive)
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => string.Equals(m.Name, methodName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (methods.Length == 0)
                return $"Method '{methodName}' not found in {className}.";

            MethodInfo matchedMethod = null;
            object[] convertedParameters = null;

            // Attempt to find a suitable method overload by converting args
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();

                // Require enough args to cover parameters
                if (args.Length < parameters.Length)
                    continue;

                object[] converted = new object[parameters.Length];
                bool convertible = true;

                // Special case: if last parameter is string, join remaining args for it
                if (parameters.Length > 0 && parameters.Last().ParameterType == typeof(string))
                {
                    // Convert all parameters except last individually
                    for (int i = 0; i < parameters.Length - 1; i++)
                    {
                        if (i >= args.Length)
                        {
                            convertible = false;
                            break;
                        }
                        try
                        {
                            converted[i] = ReflectionUtils.ParseValue(parameters[i].ParameterType, new[] { args[i] });
                        }
                        catch
                        {
                            convertible = false;
                            break;
                        }
                    }
                    if (!convertible)
                        continue;

                    // Join remaining args for last string parameter
                    string joinedLast = string.Join(" ", args.Skip(parameters.Length - 1));
                    converted[parameters.Length - 1] = joinedLast;
                }
                else
                {
                    // Exact argument count match required
                    if (parameters.Length != args.Length)
                        continue;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        try
                        {
                            converted[i] = ReflectionUtils.ParseValue(parameters[i].ParameterType, new[] { args[i] });
                        }
                        catch
                        {
                            convertible = false;
                            break;
                        }
                    }
                    if (!convertible)
                        continue;
                }

                matchedMethod = method;
                convertedParameters = converted;
                break;
            }

            if (matchedMethod == null)
                return $"No suitable overload for method '{methodName}' with {args.Length} parameters found.";

            // For instance methods, find singleton Instance property to get target instance
            if (!matchedMethod.IsStatic)
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

                if (instanceProperty == null)
                    return $"Cannot invoke instance method '{methodName}' because no singleton Instance property was found.";

                target = instanceProperty.GetValue(null);
                if (target == null)
                    return $"Singleton instance is null for '{className}'.";
            }

            try
            {
                var result = matchedMethod.Invoke(target, convertedParameters);
                return result != null ? result.ToString() : "Method invoked successfully (void or null return).";
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
    }
}
