using System;
using System.Collections.Generic;
using MLGWorks.DevConsole.Runtime.Abstractions;
using MLGWorks.DevConsole.Runtime.Core;
using MLGWorks.Utils.Logging;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace MLGWorks.DevConsole.Tests.PlayModeTests
{
    public class InputHandlerPlayModeTests
    {
        private GameObject _gameObject;
        private FakeInput _input;
        private FakeActions _actions;
        private InputHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("InputHandlerTest");
            _gameObject.SetActive(false);
            _actions = _gameObject.AddComponent<FakeActions>();
            _handler = _gameObject.AddComponent<InputHandler>();
            _input = new FakeInput();
            _handler.Configure(_input, _actions);
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
                UnityEngine.Object.DestroyImmediate(_gameObject);
        }

        [UnityTest]
        public IEnumerator ConfigureEnablesInputWhenObjectBecomesActive()
        {
            _gameObject.SetActive(true);
            yield return null;

            Assert.That(_input.EnableCount, Is.EqualTo(1));
            Assert.That(_input.Subscribed, Is.True);
        }

        [UnityTest]
        public IEnumerator ToggleInputCallsConsoleAction()
        {
            _gameObject.SetActive(true);
            yield return null;

            _input.RaiseToggle();

            Assert.That(_actions.ToggleCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ToggleInputIsIgnoredWhileFieldIsFocused()
        {
            _gameObject.SetActive(true);
            _actions.IsInputFieldFocusedValue = true;
            yield return null;

            _input.RaiseToggle();

            Assert.That(_actions.ToggleCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator SubmitInputIsIgnoredWhenConsoleIsHidden()
        {
            _gameObject.SetActive(true);
            _actions.IsVisibleValue = false;
            yield return null;

            _input.RaiseSubmit();

            Assert.That(_actions.SubmitCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator SubmitInputIsForwardedWhenConsoleIsVisible()
        {
            _gameObject.SetActive(true);
            _actions.IsVisibleValue = true;
            yield return null;

            _input.RaiseSubmit();

            Assert.That(_actions.SubmitCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator AutocompleteIsForwardedOnlyWhenVisible()
        {
            _gameObject.SetActive(true);
            yield return null;

            _input.RaiseAutocomplete();
            Assert.That(_actions.AutocompleteCount, Is.Zero);

            _actions.IsVisibleValue = true;
            _input.RaiseAutocomplete();
            Assert.That(_actions.AutocompleteCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator HistoryPreviousIsForwardedOnlyWhenVisible()
        {
            _gameObject.SetActive(true);
            yield return null;

            _input.RaisePrevious();
            Assert.That(_actions.PreviousCount, Is.Zero);

            _actions.IsVisibleValue = true;
            _input.RaisePrevious();
            Assert.That(_actions.PreviousCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator HistoryNextIsForwardedOnlyWhenVisible()
        {
            _gameObject.SetActive(true);
            yield return null;

            _input.RaiseNext();
            Assert.That(_actions.NextCount, Is.Zero);

            _actions.IsVisibleValue = true;
            _input.RaiseNext();
            Assert.That(_actions.NextCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisableStopsInputCallbacks()
        {
            _gameObject.SetActive(true);
            yield return null;

            _gameObject.SetActive(false);
            _input.RaiseSubmit();

            Assert.That(_input.DisableCount, Is.EqualTo(1));
            Assert.That(_actions.SubmitCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DestroyDisposesInputSource()
        {
            _gameObject.SetActive(true);
            yield return null;

            UnityEngine.Object.Destroy(_gameObject);
            yield return null;

            Assert.That(_input.DisposeCount, Is.EqualTo(1));
            _gameObject = null;
        }

        private sealed class FakeInput : IConsoleInput
        {
            public event Action ToggleConsole;
            public event Action SubmitCommand;
            public event Action AutoComplete;
            public event Action HistoryPrevious;
            public event Action HistoryNext;
            public IReadOnlyList<string> HistoryBindingPaths { get; } = Array.Empty<string>();
            public int EnableCount { get; private set; }
            public int DisableCount { get; private set; }
            public int DisposeCount { get; private set; }
            public bool Subscribed => ToggleConsole != null && SubmitCommand != null;

            public void Enable() => EnableCount++;
            public void Disable() => DisableCount++;
            public void Dispose() => DisposeCount++;
            public void RaiseToggle() => ToggleConsole?.Invoke();
            public void RaiseSubmit() => SubmitCommand?.Invoke();
            public void RaiseAutocomplete() => AutoComplete?.Invoke();
            public void RaisePrevious() => HistoryPrevious?.Invoke();
            public void RaiseNext() => HistoryNext?.Invoke();
        }

        private sealed class FakeActions : MonoBehaviour, IConsoleActions
        {
            public bool IsVisibleValue;
            public bool IsInputFieldFocusedValue;
            public int ToggleCount;
            public int SubmitCount;
            public int AutocompleteCount;
            public int PreviousCount;
            public int NextCount;
            public int ClearCount;
            public bool IsVisible => IsVisibleValue;
            public bool IsInputFieldFocused => IsInputFieldFocusedValue;
            public void ToggleVisibility() => ToggleCount++;
            public void SubmitInput() => SubmitCount++;
            public void RequestAutoComplete() => AutocompleteCount++;
            public void HistoryPrevious() => PreviousCount++;
            public void HistoryNext() => NextCount++;
            public void ClearLogs() => ClearCount++;
            public void AppendToOutput(string message, LogLevel? level = null) { }
        }
    }
}
