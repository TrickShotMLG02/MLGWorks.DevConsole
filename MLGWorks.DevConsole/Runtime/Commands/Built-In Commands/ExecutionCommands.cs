using MLGWorks.DevConsole.Runtime.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace MLGWorks.DevConsole.Runtime.Commands.BuiltIn
{
    public static class ExecutionCommands
    {
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

            // Get all methods with the given name (ignore case maybe?)
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => string.Equals(m.Name, methodName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (methods.Length == 0)
                return $"Method '{methodName}' not found in {className}.";

            MethodInfo matchedMethod = null;
            object[] convertedParameters = null;

            // Try to find a method overload matching args count & convertible args
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();

                // If args count is less than parameter count, skip (cannot invoke)
                if (args.Length < parameters.Length)
                    continue;

                object[] converted = new object[parameters.Length];
                bool convertible = true;

                // If last parameter is string and args are enough or more, do special conversion
                if (parameters.Length > 0 && parameters.Last().ParameterType == typeof(string))
                {
                    // Convert all but last parameter individually
                    for (int i = 0; i < parameters.Length - 1; i++)
                    {
                        if (i >= args.Length)
                        {
                            convertible = false;
                            break;
                        }

                        try
                        {
                            string[] tmpArr = new string[1];
                            tmpArr[0] = args[i];
                            converted[i] = ReflectionUtils.ParseValue(parameters[i].ParameterType, tmpArr);
                        }
                        catch
                        {
                            convertible = false;
                            break;
                        }
                    }

                    if (!convertible)
                        continue;

                    // Join all remaining args for last string parameter
                    string joinedLast = string.Join(" ", args.Skip(parameters.Length - 1));
                    converted[parameters.Length - 1] = joinedLast;
                }
                else
                {
                    // Otherwise, exact match required
                    if (parameters.Length != args.Length)
                        continue;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        try
                        {
                            string[] tmpArr = new string[1];
                            tmpArr[0] = args[i];
                            converted[i] = ReflectionUtils.ParseValue(parameters[i].ParameterType, tmpArr);
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

            // If instance method, try to get the instance via Instance property as in your pattern
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
