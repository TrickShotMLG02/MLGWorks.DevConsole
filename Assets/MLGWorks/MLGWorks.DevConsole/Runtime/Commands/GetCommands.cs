using MLGWorks.DevConsole.Runtime.Utils;
using System;
using System.Reflection;

namespace MLGWorks.DevConsole.Runtime.Commands
{
    public static class GetCommand
    {
        [Command("Get", "Gets the value of a variable of a given class")]
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
    }
}
