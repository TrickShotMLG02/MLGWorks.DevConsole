#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Configuration;
using MLGWorks.DevConsole.Runtime.Utils;

namespace MLGWorks.DevConsole.Editors
{
    internal static class DevConsoleCommandDiscovery
    {
        public static IReadOnlyList<DevConsoleCommandDefinition> Discover()
        {
            var definitions = new List<DevConsoleCommandDefinition>();

            foreach (var assembly in ReflectionUtils.GetSortedAssemblies())
            {
                if (!ShouldScanAssembly(assembly))
                    continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types.Where(type => type != null).ToArray();
                }

                foreach (var type in types.OrderBy(item => item.FullName))
                {
                    foreach (var method in type
                        .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .OrderBy(item => item.Name))
                    {
                        var attribute = method.GetCustomAttribute<CommandAttribute>();
                        if (attribute == null)
                            continue;

                        definitions.Add(new DevConsoleCommandDefinition
                        {
                            commandName = attribute.Name,
                            description = attribute.Description,
                            aliases = attribute.Aliases.ToArray(),
                            dangerLevel = attribute.DangerLevel,
                            assemblyName = assembly.GetName().Name,
                            declaringTypeName = type.FullName,
                            methodName = method.Name,
                            parameterTypeNames = method.GetParameters()
                                .Select(parameter => parameter.ParameterType.AssemblyQualifiedName)
                                .ToArray(),
                            isTestOnly = IsTestAssembly(assembly),
                            enabled = true
                        });
                    }
                }
            }

            return definitions
                .GroupBy(definition => definition.StableId)
                .Select(group => group.First())
                .OrderBy(definition => definition.commandName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(definition => definition.declaringTypeName, StringComparer.Ordinal)
                .ToArray();
        }

        private static bool ShouldScanAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return !name.StartsWith("Unity", StringComparison.Ordinal) &&
                   !name.StartsWith("UnityEditor", StringComparison.Ordinal) &&
                   !name.EndsWith(".Editor", StringComparison.Ordinal) &&
                   !name.EndsWith("-Editor", StringComparison.Ordinal) &&
                   !name.Contains("TestRunner", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTestAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            return name.EndsWith(".Tests", StringComparison.Ordinal) ||
                   name.Contains(".EditorTests", StringComparison.Ordinal) ||
                   name.Contains(".PlayModeTests", StringComparison.Ordinal);
        }
    }
}

#endif
