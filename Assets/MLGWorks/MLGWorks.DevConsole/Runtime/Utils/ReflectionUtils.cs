using System;
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
    }
}
