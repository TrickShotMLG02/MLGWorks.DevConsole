using MLGWorks.DevConsole.Runtime.Utils;
using System;
using System.Reflection;

namespace MLGWorks.DevConsole.Runtime.Commands.BuiltIn
{
    public static class GetCommand
    {
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

        [Command("set", "Sets the value of a variable of a given class to a specific value")]
        public static string Set(string className, string variableName, string[] args)
        {
            if (args.Length < 1)
            {
                return $"Usage: set<ClassName> < VariableName > < Value >";
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

                return $"set {className}.{variableName} {oldValue} => {parsedValue}";
            }
            catch (Exception ex)
            {
                return $"Failed to set value: {ex.Message}";
            }
        }
    }
}
