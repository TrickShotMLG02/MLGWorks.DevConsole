using MLGWorks.DevConsole.Runtime.Core;
using MLGWorks.DevConsole.Runtime.UI;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace MLGWorks.DevConsole.Tests.EditorTests
{
    public class DevConsoleEditorTests
    {
        private const string PrefabPath = "Assets/MLGWorks/MLGWorks.DevConsole/Prefabs/DevConsole.prefab";
        private const string InputActionsPath = "Assets/MLGWorks/MLGWorks.DevConsole/Resources/DevConsoleInputActions.inputactions";

        [Test]
        public void DevConsolePrefabExists()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), Is.Not.Null);
        }

        [Test]
        public void DevConsolePrefabContainsRequiredComponents()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

            Assert.That(prefab.GetComponents<MonoBehaviour>().Any(component => component.GetType().Name == "DevConsole"), Is.True);
            Assert.That(prefab.GetComponent<ConsoleUI>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<InputHandler>(), Is.Not.Null);
        }

        [Test]
        public void ConsoleUiPrefabReferencesAreAssigned()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var ui = prefab.GetComponent<ConsoleUI>();
            var serialized = new SerializedObject(ui);

            Assert.That(serialized.FindProperty("_consoleCanvas").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_inputField").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_outputText").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_scrollRect").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_autocompleteText").objectReferenceValue, Is.Not.Null);
        }

        [Test]
        public void InputActionsAssetContainsExpectedMapAndActions()
        {
            var assetText = File.ReadAllText(InputActionsPath);
            StringAssert.Contains("DevConsole", assetText);
            StringAssert.Contains("ToggleConsole", assetText);
            StringAssert.Contains("SubmitCommand", assetText);
            StringAssert.Contains("AutoComplete", assetText);
            StringAssert.Contains("CommandHistoryPrevious", assetText);
            StringAssert.Contains("CommandHistoryNext", assetText);
        }

        [Test]
        public void InputActionsAssetHasKeyboardBindings()
        {
            var assetText = File.ReadAllText(InputActionsPath);
            Assert.That(assetText, Does.Contain("<Keyboard>"));
            Assert.That(assetText, Does.Contain("upArrow"));
            Assert.That(assetText, Does.Contain("downArrow"));
        }
    }
}
