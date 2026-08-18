#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Configuration;
using MLGWorks.DevConsole.Runtime.Core;
using UnityEditor;
using UnityEngine;

namespace MLGWorks.DevConsole.Editors
{
    public sealed class DevConsoleCommandWindow : EditorWindow
    {
        private DevConsoleCommandSettings _settings;
        private Vector2 _scrollPosition;
        private string _search = string.Empty;
        private readonly Dictionary<string, bool> _expandedTypes = new();
        private GUIStyle _searchHintLabelStyle;
        private GUIStyle _metricsStyle;

        [MenuItem("Window/MLGWorks/DevConsole Commands")]
        public static void ShowWindow()
        {
            GetWindow<DevConsoleCommandWindow>("DevConsole Commands");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("DevConsole Command Catalog", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Commands are discovered in the editor and registered at runtime from the selected catalog. " +
                "Disabled commands and classes are excluded from execution, help, and autocomplete.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            _settings = (DevConsoleCommandSettings)EditorGUILayout.ObjectField(
                "Command Settings", _settings, typeof(DevConsoleCommandSettings), false);
            if (EditorGUI.EndChangeCheck())
                Repaint();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Settings Asset"))
                    CreateSettingsAsset();

                using (new EditorGUI.DisabledScope(_settings == null))
                {
                    if (GUILayout.Button("Refresh Discovery"))
                        RefreshDiscovery();
                        using (new EditorGUI.DisabledScope(_settings == null || _settings.GetObsoleteCommands().Count == 0))
                    {
                        if (GUILayout.Button($"Remove Obsolete ({_settings?.GetObsoleteCommands().Count ?? 0})"))
                            RemoveObsoleteCommands();
                    }
                    if (GUILayout.Button("Assign To Selected Console"))
                        AssignToSelectedConsole();
                }
            }

            GUILayout.Space(8f);

            if (_settings == null)
                return;

            _search = EditorGUILayout.TextField("Search", _search);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Search prefixes:", SearchHintLabelStyle);
                EditorGUILayout.LabelField("No prefix = command names", SearchHintLabelStyle);
                EditorGUILayout.LabelField("$ = classes and namespaces", SearchHintLabelStyle);
                EditorGUILayout.LabelField("# = all catalog metadata", SearchHintLabelStyle);
            }

            GUILayout.Space(6f);

            EditorGUILayout.LabelField(
                $"Commands: {_settings.GetCommandCount()} | Disabled: {_settings.GetDisabledCommandCount()} | " +
                $"Obsolete: {_settings.GetObsoleteCommandCount()} | " +
                $"Disabled classes: {_settings.DisabledTypeNames.Count}",
                MetricsStyle);

            GUILayout.Space(8f);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (var group in FilteredGroups())
                DrawTypeGroup(group.Key, group.ToList());
            EditorGUILayout.EndScrollView();
        }

        private IEnumerable<IGrouping<string, DevConsoleCommandDefinition>> FilteredGroups()
        {
            var query = _settings.Commands.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(_search))
            {
                string search = _search.Trim();
                char prefix = search[0];
                string term = prefix is '$' or '#' ? search.Substring(1).Trim() : search;

                if (term.Length == 0)
                    return Enumerable.Empty<IGrouping<string, DevConsoleCommandDefinition>>();

                query = prefix switch
                {
                    '$' => query.Where(command => Matches(command.declaringTypeName, term)),
                    '#' => query.Where(command =>
                        Matches(command.commandName, term) ||
                        Matches(command.declaringTypeName, term) ||
                        Matches(command.assemblyName, term) ||
                        Matches(command.methodName, term)),
                    _ => query.Where(command => Matches(command.commandName, term))
                };
            }

            return query.GroupBy(command => command.declaringTypeName)
                .OrderBy(group => group.Key, System.StringComparer.Ordinal);
        }

        private static bool Matches(string value, string search)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private GUIStyle SearchHintLabelStyle => _searchHintLabelStyle ??= new GUIStyle(EditorStyles.label)
        {
            fontSize = 12
        };

        private GUIStyle MetricsStyle => _metricsStyle ??= new GUIStyle(EditorStyles.label)
        {
            fontSize = 12
        };

        private void DrawTypeGroup(string typeName, List<DevConsoleCommandDefinition> commands)
        {
            if (!_expandedTypes.ContainsKey(typeName))
                _expandedTypes[typeName] = true;

            bool testOnlyCategory = commands.All(command => _settings.IsCommandTestOnly(command));
            var categoryDanger = commands
                .Where(command => !command.isObsolete)
                .Select(command => command.dangerLevel)
                .DefaultIfEmpty(CommandDangerLevel.None)
                .Max();
            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = GetDangerColor(categoryDanger, previousBackground);
            try
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        _expandedTypes[typeName] = EditorGUILayout.Foldout(_expandedTypes[typeName], typeName, true);
                        using (new EditorGUI.DisabledScope(testOnlyCategory))
                        {
                            bool enabled = !_settings.IsTypeDisabled(typeName);
                            bool newEnabled = EditorGUILayout.ToggleLeft("Enabled", enabled, GUILayout.Width(70));
                            if (newEnabled != enabled)
                                UpdateTypeEnabled(typeName, newEnabled);
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(18f);
                        using (new EditorGUI.DisabledScope(testOnlyCategory))
                        {
                            var eligibleCommands = commands
                                .Where(command => !command.isObsolete && !_settings.IsCommandTestOnly(command))
                                .ToList();
                            bool enableAll = eligibleCommands.Any(command => !command.enabled);
                            if (GUILayout.Button(enableAll ? "Enable All" : "Disable All", GUILayout.Width(90)))
                                UpdateCommandsEnabled(eligibleCommands, enableAll);
                        }
                    }

                    if (!_expandedTypes[typeName])
                        return;

                    foreach (var command in commands)
                    {
                        bool isObsolete = command.isObsolete;
                        bool isTestOnly = _settings.IsCommandTestOnly(command);
                        bool enabled = command.enabled && !_settings.IsTypeDisabled(typeName);
                        string label = isObsolete
                            ? $"[Obsolete] {command.commandName} - no longer discovered"
                            : isTestOnly
                                ? $"[Test-only] {command.commandName} - disabled outside tests"
                                : $"{command.commandName} - {command.description}";

                        Color previousRowBackground = GUI.backgroundColor;
                        GUI.backgroundColor = GetDangerColor(command.dangerLevel, previousRowBackground);
                        try
                        {
                            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                            {
                                GUILayout.Space(18f);
                                using (new EditorGUI.DisabledScope(isObsolete || isTestOnly || _settings.IsTypeDisabled(typeName)))
                                {
                                    bool newEnabled = EditorGUILayout.ToggleLeft(label, enabled);
                                    if (!isObsolete && !isTestOnly && newEnabled != command.enabled && !_settings.IsTypeDisabled(typeName))
                                        UpdateCommandEnabled(command, newEnabled);
                                }
                            }
                        }
                        finally
                        {
                            GUI.backgroundColor = previousRowBackground;
                        }
                    }
                }
            }
            finally
            {
                GUI.backgroundColor = previousBackground;
            }
        }

        private void UpdateCommandsEnabled(IEnumerable<DevConsoleCommandDefinition> commands, bool enabled)
        {
            Undo.RecordObject(_settings, enabled ? "Enable DevConsole commands" : "Disable DevConsole commands");
            foreach (var command in commands)
            {
                if (!command.isObsolete && !_settings.IsCommandTestOnly(command))
                    _settings.SetCommandEnabled(command.StableId, enabled);
            }

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
        }

        private static Color GetDangerColor(CommandDangerLevel dangerLevel, Color fallback)
        {
            return dangerLevel switch
            {
                CommandDangerLevel.Warning => new Color(1f, 0.85f, 0.25f),
                CommandDangerLevel.Dangerous => new Color(1f, 0.45f, 0.45f),
                _ => fallback
            };
        }

        private void CreateSettingsAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create DevConsole Command Settings", "DevConsoleCommandSettings", "asset", "Choose a location.");
            if (string.IsNullOrEmpty(path))
                return;

            _settings = CreateInstance<DevConsoleCommandSettings>();
            AssetDatabase.CreateAsset(_settings, path);
            AssetDatabase.SaveAssets();
            RefreshDiscovery();
            Selection.activeObject = _settings;
        }

        private void RefreshDiscovery()
        {
            if (_settings == null)
                return;

            Undo.RecordObject(_settings, "Refresh DevConsole command discovery");
            _settings.ReplaceCommands(DevConsoleCommandDiscovery.Discover());
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            Repaint();
        }

        private void UpdateTypeEnabled(string typeName, bool enabled)
        {
            Undo.RecordObject(_settings, "Toggle DevConsole command class");
            _settings.SetTypeDisabled(typeName, !enabled);
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
        }

        private void UpdateCommandEnabled(DevConsoleCommandDefinition command, bool enabled)
        {
            Undo.RecordObject(_settings, "Toggle DevConsole command");
            _settings.SetCommandEnabled(command.StableId, enabled);
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
        }

        private void RemoveObsoleteCommands()
        {
            Undo.RecordObject(_settings, "Remove obsolete DevConsole commands");
            _settings.RemoveObsoleteCommands();
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            Repaint();
        }

        private void AssignToSelectedConsole()
        {
            var console = Selection.activeGameObject?.GetComponent<MLGWorks.DevConsole.Runtime.Core.DevConsole>();
            if (console == null)
            {
                EditorUtility.DisplayDialog("DevConsole", "Select a DevConsole GameObject first.", "OK");
                return;
            }

            var serialized = new SerializedObject(console);
            serialized.FindProperty("_commandSettings").objectReferenceValue = _settings;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(console);
        }
    }
}

#endif
