#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace MLGWorks.DevConsole.Editors
{
    public class DevConsoleWindow : EditorWindow
    {
        [MenuItem("Window/MLGWorks/DevConsole Config")]
        public static void ShowWindow()
        {
            GetWindow<DevConsoleWindow>("DevConsole Config");
        }

        private void OnGUI()
        {
            GUILayout.Label("DevConsole Settings", EditorStyles.boldLabel);
            // Add inspector fields for customizing console
        }
    }
}

#endif
