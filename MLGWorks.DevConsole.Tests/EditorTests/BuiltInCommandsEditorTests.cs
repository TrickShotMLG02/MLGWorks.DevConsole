using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using MLGWorks.DevConsole.Runtime.Abstractions;
using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Commands.BuiltIn;
using MLGWorks.Utils.Logging;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.EditorTests
{
    public class BuiltInCommandsEditorTests
    {
        [SetUp]
        public void SetUp()
        {
            CommandManager.RegisterAll();
            VariableTargets.IntValue = 12;
            VariableTargets.Enabled = false;
            VariableTargets.Mode = TestMode.Alpha;
            VariableTargets.Numbers = Array.Empty<int>();
            VariableTargets.Values = new Dictionary<string, int>();
            VariableTargets.Name = "initial";
        }

        [TearDown]
        public void TearDown()
        {
            CommandManager.Output = null;
        }

        [Test]
        public void HelpListsRegisteredCommandsAndUsage()
        {
            var help = BaseCommands.Help();

            StringAssert.Contains("help", help);
            StringAssert.Contains("Usage:", help);
            StringAssert.Contains("add", help);
        }

        [Test]
        public void CommandsListsCommandSchemesWithoutDescriptions()
        {
            var commands = BaseCommands.Commands();

            StringAssert.Contains("add <float a> <float b>", commands);
            Assert.That(commands, Does.Not.Contain("Adds two numbers"));
        }

        [Test]
        public void ClearUsesConfiguredConsoleActions()
        {
            var actions = new RecordingActions();
            CommandManager.Output = actions;

            BaseCommands.Clear();

            Assert.That(actions.ClearCount, Is.EqualTo(1));
        }

        [Test]
        public void CloseUsesConfiguredConsoleActions()
        {
            var actions = new RecordingActions();
            CommandManager.Output = actions;

            BaseCommands.CloseConsole();

            Assert.That(actions.ToggleCount, Is.EqualTo(1));
        }

        [Test]
        public void ClearAndCloseAreSafeWithoutConsoleOutput()
        {
            Assert.DoesNotThrow(() => BaseCommands.Clear());
            Assert.DoesNotThrow(() => BaseCommands.CloseConsole());
        }

        [Test]
        public void TimeUsesExpectedInvariantShape()
        {
            Assert.That(BaseCommands.Time(), Does.Match("^\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2}$"));
        }

        [Test]
        public void UptimeReturnsLabeledNonnegativeValue()
        {
            var result = BaseCommands.Uptime();

            StringAssert.StartsWith("Uptime: ", result);
            Assert.That(result, Does.Not.Contain("-"));
        }

        [Test]
        public void PlatformReportsEditorInEditorTests()
        {
            Assert.That(BaseCommands.Platform(), Is.EqualTo("Editor"));
        }

        [Test]
        public void VersionContainsProductNameAndVersion()
        {
            var result = BaseCommands.Version();

            StringAssert.Contains(UnityEngine.Application.productName, result);
            StringAssert.Contains(UnityEngine.Application.version, result);
        }

        [Test]
        public void InvokeCallsStaticMethodsWithConvertedArguments()
        {
            var result = ExecutionCommands.Invoke(
                $"{typeof(InvocationTargets).FullName}.Add", new[] { "2", "3" });

            Assert.That(result, Is.EqualTo("5"));
        }

        [Test]
        public void InvokeMatchesStaticMethodNamesCaseInsensitively()
        {
            var result = ExecutionCommands.Invoke(
                $"{typeof(InvocationTargets).FullName}.aDd", new[] { "2", "3" });

            Assert.That(result, Is.EqualTo("5"));
        }

        [Test]
        public void InvokeJoinsRemainingArgumentsForTrailingString()
        {
            var result = ExecutionCommands.Invoke(
                $"{typeof(InvocationTargets).FullName}.Message", new[] { "hello", "editor", "tests" });

            Assert.That(result, Is.EqualTo("hello editor tests"));
        }

        [Test]
        public void InvokeSupportsBooleanAndEnumConversion()
        {
            var boolResult = ExecutionCommands.Invoke(
                $"{typeof(InvocationTargets).FullName}.Describe", new[] { "yes", "Beta" });

            Assert.That(boolResult, Is.EqualTo("True:Beta"));
        }

        [Test]
        public void InvokeReportsMissingMethod()
        {
            var name = $"{typeof(InvocationTargets).FullName}.Missing";

            Assert.That(ExecutionCommands.Invoke(name), Is.EqualTo($"Method '{name}' not found."));
        }

        [Test]
        public void InvokeReportsMalformedMethodName()
        {
            Assert.That(ExecutionCommands.Invoke("Malformed", Array.Empty<string>()),
                Is.EqualTo("Method 'Malformed' not found."));
        }

        [Test]
        public void InvokeRejectsEmptyMethodNames()
        {
            Assert.That(ExecutionCommands.Invoke(null), Is.EqualTo("Method name cannot be empty."));
            Assert.That(ExecutionCommands.Invoke("   "), Is.EqualTo("Method name cannot be empty."));
        }

        [Test]
        public void InvokeReportsMissingArguments()
        {
            var result = ExecutionCommands.Invoke(
                $"{typeof(InvocationTargets).FullName}.Add", new[] { "2" });

            StringAssert.Contains("Not enough arguments", result);
        }

        [Test]
        public void InvokeReportsExtraArgumentsForFixedSignature()
        {
            var result = ExecutionCommands.Invoke(
                $"{typeof(InvocationTargets).FullName}.Add", new[] { "2", "3", "4" });

            StringAssert.Contains("expects 2 arguments", result);
        }

        [Test]
        public void InvokeReportsConversionFailures()
        {
            var result = ExecutionCommands.Invoke(
                $"{typeof(InvocationTargets).FullName}.Add", new[] { "bad", "3" });

            Assert.That(result, Is.EqualTo("Failed to convert arguments for method 'Add'."));
        }

        [Test]
        public void InvokeReportsTargetExceptions()
        {
            var result = ExecutionCommands.Invoke(
                $"{typeof(InvocationTargets).FullName}.Throw", Array.Empty<string>());

            Assert.That(result, Is.EqualTo("Exception thrown by method: expected invoke failure"));
        }

        [Test]
        public void InvokeReportsVoidMethodsAsSuccessful()
        {
            var result = ExecutionCommands.Invoke(
                $"{typeof(InvocationTargets).FullName}.NoReturn", Array.Empty<string>());

            Assert.That(result, Is.EqualTo("Method invoked successfully (void or null return)."));
        }

        [Test]
        public void ResolveFindsStaticMethod()
        {
            Assert.That(ExecutionCommands.TryResolveTargetAndMethod(
                $"{typeof(InvocationTargets).FullName}.Add", out var target, out var method), Is.True);
            Assert.That(target, Is.Null);
            Assert.That(method.Name, Is.EqualTo("Add"));
        }

        [Test]
        public void ResolveTraversesSingletonMembers()
        {
            var path = $"{typeof(SingletonTargets).FullName}.Child.Ping";

            Assert.That(ExecutionCommands.TryResolveTargetAndMethod(path, out var target, out var method), Is.True);
            Assert.That(target, Is.SameAs(SingletonTargets.Instance.Child));
            Assert.That(method.Invoke(target, null), Is.EqualTo("pong"));
        }

        [Test]
        public void ResolveRejectsMissingTypeMemberAndMethod()
        {
            Assert.That(ExecutionCommands.TryResolveTargetAndMethod("Missing.Type.Method", out _, out _), Is.False);
            Assert.That(ExecutionCommands.TryResolveTargetAndMethod(
                $"{typeof(SingletonTargets).FullName}.Missing.Ping", out _, out _), Is.False);
            Assert.That(ExecutionCommands.TryResolveTargetAndMethod(
                $"{typeof(InvocationTargets).FullName}.Missing", out _, out _), Is.False);
        }

        [Test]
        public void GetReadsStaticFieldsAndProperties()
        {
            Assert.That(VariableCommands.Get(nameof(VariableTargets), nameof(VariableTargets.IntValue)),
                Is.EqualTo("VariableTargets.IntValue = 12"));
            Assert.That(VariableCommands.Get(nameof(VariableTargets), nameof(VariableTargets.Name)),
                Is.EqualTo("VariableTargets.Name = initial"));
        }

        [Test]
        public void GetReportsMissingTypeAndMember()
        {
            const string missingType = "DefinitelyMissingType_DevConsole_BuiltIn";
            Assert.That(VariableCommands.Get(missingType, "Value"), Is.EqualTo($"Type '{missingType}' not found."));
            Assert.That(VariableCommands.Get(nameof(VariableTargets), "Missing"),
                Is.EqualTo("Variable 'Missing' not found in VariableTargets."));
        }

        [Test]
        public void SetWritesFieldsAndProperties()
        {
            var fieldResult = VariableCommands.Set(nameof(VariableTargets), nameof(VariableTargets.IntValue), new[] { "42" });
            var propertyResult = VariableCommands.Set(nameof(VariableTargets), nameof(VariableTargets.Name), new[] { "changed" });

            Assert.That(fieldResult, Is.EqualTo("set VariableTargets.IntValue 12 => 42"));
            Assert.That(propertyResult, Is.EqualTo("set VariableTargets.Name initial => changed"));
            Assert.That(VariableTargets.IntValue, Is.EqualTo(42));
            Assert.That(VariableTargets.Name, Is.EqualTo("changed"));
        }

        [Test]
        public void SetParsesBooleansEnumsArraysAndDictionaries()
        {
            VariableCommands.Set(nameof(VariableTargets), nameof(VariableTargets.Enabled), new[] { "yes" });
            VariableCommands.Set(nameof(VariableTargets), nameof(VariableTargets.Mode), new[] { "Beta" });
            VariableCommands.Set(nameof(VariableTargets), nameof(VariableTargets.Numbers), new[] { "1", "2", "3" });
            VariableCommands.Set(nameof(VariableTargets), nameof(VariableTargets.Values), new[] { "one=1", "two=2" });

            Assert.That(VariableTargets.Enabled, Is.True);
            Assert.That(VariableTargets.Mode, Is.EqualTo(TestMode.Beta));
            Assert.That(VariableTargets.Numbers, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(VariableTargets.Values["two"], Is.EqualTo(2));
        }

        [Test]
        public void SetReportsMissingValueTypeMemberAndReadOnlyProperty()
        {
            Assert.That(VariableCommands.Set(nameof(VariableTargets), nameof(VariableTargets.IntValue), Array.Empty<string>()),
                Is.EqualTo("Usage: set <ClassName> <VariableName> <Value>"));
            const string missingType = "DefinitelyMissingType_DevConsole_BuiltIn";
            Assert.That(VariableCommands.Set(missingType, "Value", new[] { "1" }),
                Is.EqualTo($"Type '{missingType}' not found."));
            Assert.That(VariableCommands.Set(nameof(VariableTargets), "Missing", new[] { "1" }),
                Is.EqualTo("Variable 'Missing' not found in VariableTargets."));
            Assert.That(VariableCommands.Set(nameof(VariableTargets), nameof(VariableTargets.ReadOnly), new[] { "x" }),
                Is.EqualTo("Property 'ReadOnly' is read-only."));
        }

        [Test]
        public void SetReportsInvalidValues()
        {
            var result = VariableCommands.Set(nameof(VariableTargets), nameof(VariableTargets.IntValue), new[] { "invalid" });

            StringAssert.StartsWith("Failed to set value:", result);
        }

        private sealed class RecordingActions : IConsoleActions
        {
            public int ClearCount;
            public int ToggleCount;
            public bool IsVisible => false;
            public bool IsInputFieldFocused => false;
            public void AppendToOutput(string message, LogLevel? level = null) { }
            public void ToggleVisibility() => ToggleCount++;
            public void SubmitInput() { }
            public void RequestAutoComplete() { }
            public void HistoryPrevious() { }
            public void HistoryNext() { }
            public void ClearLogs() => ClearCount++;
        }
    }

    public enum TestMode
    {
        Alpha,
        Beta
    }

    public static class InvocationTargets
    {
        public static int Add(int a, int b) => a + b;
        public static string Message(string message) => message;
        public static string Describe(bool enabled, TestMode mode) => $"{enabled}:{mode}";
        public static void NoReturn() { }
        public static void Throw() => throw new InvalidOperationException("expected invoke failure");
    }

    public sealed class SingletonTargets
    {
        public static SingletonTargets Instance { get; } = new SingletonTargets();
        public NestedTarget Child { get; } = new NestedTarget();
    }

    public sealed class NestedTarget
    {
        public string Ping() => "pong";
    }

    public static class VariableTargets
    {
        public static int IntValue = 12;
        public static bool Enabled { get; set; }
        public static TestMode Mode { get; set; } = TestMode.Alpha;
        public static int[] Numbers { get; set; } = Array.Empty<int>();
        public static Dictionary<string, int> Values { get; set; } = new Dictionary<string, int>();
        public static string Name { get; set; } = "initial";
        public static string ReadOnly => "fixed";
    }
}
