using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MLGWorks.DevConsole.Runtime.Configuration
{
    /// <summary>
    /// Serialized command catalog generated and maintained by the editor.
    /// </summary>
    [CreateAssetMenu(fileName = "DevConsoleCommandSettings", menuName = "MLGWorks/DevConsole Command Settings")]
    public sealed class DevConsoleCommandSettings : ScriptableObject
    {
        [SerializeField] private List<DevConsoleCommandDefinition> _commands = new();
        [SerializeField] private List<string> _disabledTypeNames = new();

        public IReadOnlyList<DevConsoleCommandDefinition> Commands => _commands;
        public IReadOnlyCollection<string> DisabledTypeNames => _disabledTypeNames;

        public bool IsCommandTestOnly(DevConsoleCommandDefinition command) =>
            command != null && IsTestOnly(command);

        public int GetCommandCount() =>
            _commands.Count(command => command != null && !command.isObsolete && !IsTestOnly(command));

        public int GetObsoleteCommandCount() =>
            _commands.Count(command => command != null && command.isObsolete && !IsTestOnly(command));

        public bool IsTypeDisabled(string declaringTypeName) =>
            !string.IsNullOrWhiteSpace(declaringTypeName) && _disabledTypeNames.Contains(declaringTypeName);

        public void SetTypeDisabled(string declaringTypeName, bool disabled)
        {
            if (string.IsNullOrWhiteSpace(declaringTypeName))
                return;

            if (disabled)
            {
                if (!_disabledTypeNames.Contains(declaringTypeName))
                    _disabledTypeNames.Add(declaringTypeName);
            }
            else
            {
                _disabledTypeNames.Remove(declaringTypeName);
            }
        }

        public bool SetCommandEnabled(string stableId, bool enabled)
        {
            var command = _commands.FirstOrDefault(item => item != null && item.StableId == stableId);
            if (command == null)
                return false;

            command.enabled = enabled;
            return true;
        }

        public void ReplaceCommands(IEnumerable<DevConsoleCommandDefinition> commands)
        {
            var discoveredCommands = commands?.Where(command => command != null).ToList() ??
                                     new List<DevConsoleCommandDefinition>();
            var previousCommands = _commands
                .Where(item => item != null)
                .GroupBy(item => item.StableId)
                .ToDictionary(group => group.Key, group => group.First());
            var discoveredIds = discoveredCommands.Select(item => item.StableId).ToHashSet();

            foreach (var command in discoveredCommands)
            {
                if (previousCommands.TryGetValue(command.StableId, out var previous))
                {
                    command.enabled = previous.enabled;
                    command.isObsolete = false;
                }
            }

            var obsoleteCommands = _commands
                .Where(command => command != null)
                .Where(command => !discoveredIds.Contains(command.StableId))
                .ToList();

            foreach (var command in obsoleteCommands)
                command.isObsolete = true;

            _commands = discoveredCommands;
            _commands.AddRange(obsoleteCommands);
        }

        public IEnumerable<DevConsoleCommandDefinition> GetEnabledCommands()
        {
            return _commands.Where(command =>
                command != null && command.enabled && !command.isObsolete &&
                !IsTestOnly(command) &&
                !IsTypeDisabled(command.declaringTypeName));
        }

        public IReadOnlyList<DevConsoleCommandDefinition> GetObsoleteCommands() =>
            _commands.Where(command => command != null && command.isObsolete).ToArray();

        public int GetDisabledCommandCount()
        {
            return _commands.Count(command =>
                command != null && !command.isObsolete &&
                !IsTestOnly(command) &&
                (!command.enabled || IsTypeDisabled(command.declaringTypeName)));
        }

        private static bool IsTestOnly(DevConsoleCommandDefinition command)
        {
            if (command.isTestOnly)
                return true;

            var assemblyName = command.assemblyName ?? string.Empty;
            return assemblyName.EndsWith(".Tests", StringComparison.Ordinal) ||
                   assemblyName.Contains(".EditorTests", StringComparison.Ordinal) ||
                   assemblyName.Contains(".PlayModeTests", StringComparison.Ordinal);
        }

        public int RemoveObsoleteCommands()
        {
            int removed = _commands.RemoveAll(command => command.isObsolete);
            return removed;
        }
    }
}
