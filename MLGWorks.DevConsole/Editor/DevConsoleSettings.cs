#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace MLGWorks.DevConsole.Editors
{
    [CreateAssetMenu(fileName = "DevConsoleSettings", menuName = "MLGWorks/DevConsole Settings")]
    public class DevConsoleSettings : ScriptableObject
    {
        public KeyCode toggleKey = KeyCode.BackQuote;
        public int maxScrollback = 200;
        public string prompt = ">> ";
    }
}

#endif
