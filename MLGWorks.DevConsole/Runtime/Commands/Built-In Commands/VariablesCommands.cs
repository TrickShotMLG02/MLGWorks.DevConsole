using MLGWorks.DevConsole.Runtime.Utils;
using System;
using System.Reflection;

namespace MLGWorks.DevConsole.Runtime.Commands.BuiltIn
{
    /// <summary>
    /// Provides built-in console commands to get or set static or instance variables/properties by reflection.
    /// </summary>
    public static class GetCommand
    {
        /// <summary>
        /// Gets the value of a variable or property of a given class.
        /// </summary>
        /// <param name="className">The name of the class containing the variable/property.</param>
        /// <param name="variableName">The name of the variable or property to get.</param>
        /// <param name="args">Optional arguments (not used here, but accepted for command signature compatibility).</param>
        /// <returns>A string describing the current value or an error message if not found.</returns>
        [Command("get", "Gets the value of a variable of a given class")]
        public static string Get(string className, string variableName, string[] args = default)
        {
            Type type = ReflectionUtils.FindType(className);
            if (type == null)
            {
                return $"Type '{className}' not found.";
            }

            object target = null;
            var member = ReflectionUtils.FindMember(type, variableName, out target);

            if (member == null)
            {
                return $"Variable '{variableName}' not found in {className}.";
            }

            object value = member is FieldInfo fi
                ? fi.GetValue(target)
                : ((PropertyInfo)member).GetValue(target);

            return $"{className}.{variableName} = {value}";
        }

        /// <summary>
        /// Sets the value of a variable or property of a given class.
        /// </summary>
        /// <param name="className">The name of the class containing the variable/property.</param>
        /// <param name="variableName">The name of the variable or property to set.</param>
        /// <param name="args">The value(s) to assign, parsed to the correct type.</param>
        /// <returns>A string describing the change or an error message if the operation failed.</returns>
        [Command("set", "Sets the value of a variable of a given class to a specific value")]
        public static string Set(string className, string variableName, string[] args)
        {
            if (args.Length < 1)
            {
                return $"Usage: set <ClassName> <VariableName> <Value>";
            }

            Type type = ReflectionUtils.FindType(className);
            if (type == null)
            {
                return $"Type '{className}' not found.";
            }

            object target = null;
            var member = ReflectionUtils.FindMember(type, variableName, out target);

            if (member == null)
            {
                return $"Variable '{variableName}' not found in {className}.";
            }

            Type valueType = member is FieldInfo fi ? fi.FieldType : ((PropertyInfo)member).PropertyType;

            try
            {
                object parsedValue = ReflectionUtils.ParseValue(valueType, args);

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
                else
                {
                    return $"Property '{variableName}' is read-only.";
                }

                return $"set {className}.{variableName} {oldValue} => {parsedValue}";
            }
            catch (Exception ex)
            {
                return $"Failed to set value: {ex.Message}";
            }
        }
    }
}
