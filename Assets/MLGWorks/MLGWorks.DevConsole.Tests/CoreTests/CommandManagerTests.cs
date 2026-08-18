using System;
using System.Collections.Generic;
using MLGWorks.DevConsole.Runtime.Abstractions;
using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.Utils.Logging;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.CoreTests
{
    public class CommandManagerTests
    {
        private RecordingOutput _output;

        [SetUp]
        public void SetUp()
        {
            CommandManager.RegisterAll();
            _output = new RecordingOutput();
        }

        [TearDown]
        public void TearDown()
        {
            CommandManager.Output = null;
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void EmptyInputFailsWithoutOutput(string input)
        {
            Assert.That(CommandManager.TryExecute(input, _output, out var result), Is.False);
            Assert.That(result, Is.Empty);
            Assert.That(_output.Messages, Is.Empty);
        }

        [Test]
        public void UnknownCommandReturnsNormalizedNameAndEchoesInput()
        {
            Assert.That(CommandManager.TryExecute("UNKNOWN", _output, out var result), Is.False);

            Assert.That(result, Is.EqualTo("Unknown command: unknown"));
            Assert.That(_output.Messages, Has.Count.EqualTo(1));
            Assert.That(_output.Messages[0], Is.EqualTo("> UNKNOWN"));
        }

        [Test]
        public void CommandEchoUsesCommandLogLevel()
        {
            CommandManager.TryExecute("test-echo hello", _output, out _);

            Assert.That(_output.Levels[0], Is.EqualTo(LogLevel.Command));
        }

        [Test]
        public void QuotedArgumentsRemainSingleArguments()
        {
            Assert.That(CommandManager.TryExecute("test-echo \"hello world\"", _output, out var result), Is.True);
            Assert.That(result, Is.EqualTo("hello world"));
        }

        [Test]
        public void RepeatedWhitespaceDoesNotCreateEmptyArguments()
        {
            Assert.That(CommandManager.TryExecute("test-add   2    3", _output, out var result), Is.True);
            Assert.That(result, Is.EqualTo("5"));
        }

        [Test]
        public void EscapedWhitespaceIsPreservedInsideArgument()
        {
            Assert.That(CommandManager.TryExecute("test-echo hello\\ world", _output, out var result), Is.True);
            Assert.That(result, Is.EqualTo("hello world"));
        }

        [Test]
        public void OptionalArgumentsUseTheirDefaultValue()
        {
            Assert.That(CommandManager.TryExecute("test-optional", _output, out var result), Is.True);
            Assert.That(result, Is.EqualTo("7"));
        }

        [Test]
        public void OptionalArgumentsCanBeOverridden()
        {
            Assert.That(CommandManager.TryExecute("test-optional 9", _output, out var result), Is.True);
            Assert.That(result, Is.EqualTo("9"));
        }

        [Test]
        public void MissingRequiredArgumentsReturnsUsage()
        {
            Assert.That(CommandManager.TryExecute("test-add 2", _output, out var result), Is.False);
            StringAssert.StartsWith("Usage: test-add", result);
        }

        [Test]
        public void ExtraArgumentsAreRejected()
        {
            Assert.That(CommandManager.TryExecute("test-add 1 2 3", _output, out var result), Is.False);
            StringAssert.StartsWith("Usage: test-add", result);
        }

        [Test]
        public void ParamsArgumentsAcceptZeroValues()
        {
            Assert.That(CommandManager.TryExecute("test-join", _output, out var result), Is.True);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ParamsArgumentsAcceptManyValues()
        {
            Assert.That(CommandManager.TryExecute("test-join one two three", _output, out var result), Is.True);
            Assert.That(result, Is.EqualTo("one|two|three"));
        }

        [Test]
        public void StringArrayArgumentsConsumeRemainingValues()
        {
            Assert.That(CommandManager.TryExecute("test-array one \"two three\"", _output, out var result), Is.True);
            Assert.That(result, Is.EqualTo("one|two three"));
        }

        [TestCase("yes", true)]
        [TestCase("1", true)]
        [TestCase("no", false)]
        [TestCase("0", false)]
        public void BooleanArgumentsUseExtendedForms(string value, bool expected)
        {
            Assert.That(CommandManager.TryExecute($"test-bool {value}", _output, out var result), Is.True);
            Assert.That(result, Is.EqualTo(expected.ToString()));
        }

        [Test]
        public void InvalidConversionReturnsCommandError()
        {
            Assert.That(CommandManager.TryExecute("test-add invalid 2", _output, out var result), Is.False);
            StringAssert.StartsWith("Command error:", result);
        }

        [Test]
        public void CommandExceptionsAreReturnedAsErrors()
        {
            Assert.That(CommandManager.TryExecute("test-throw", _output, out var result), Is.False);
            Assert.That(result, Is.EqualTo("Command error: expected failure"));
        }

        [Test]
        public void AliasesResolveToTheSameCommand()
        {
            Assert.That(CommandManager.TryExecute("te hello", _output, out var result), Is.True);
            Assert.That(result, Is.EqualTo("hello"));
        }

        [Test]
        public void UnregisterByAliasRemovesPrimaryAndAliases()
        {
            Assert.That(CommandManager.UnregisterCommand("te"), Is.True);
            Assert.That(CommandManager.Commands.ContainsKey("test-echo"), Is.False);
            Assert.That(CommandManager.Commands.ContainsKey("te"), Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("missing-command")]
        public void UnregisterMissingCommandReturnsFalse(string name)
        {
            Assert.That(CommandManager.UnregisterCommand(name), Is.False);
        }

        [Command("test-echo", "Echoes a value", "te")]
        private static string Echo(string value) => value;

        [Command("test-add")]
        private static int Add(int left, int right) => left + right;

        [Command("test-optional")]
        private static int Optional(int value = 7) => value;

        [Command("test-join")]
        private static string Join(params string[] values) => string.Join("|", values);

        [Command("test-array")]
        private static string Array(string[] values) => string.Join("|", values);

        [Command("test-bool")]
        private static bool Bool(bool value) => value;

        [Command("test-throw")]
        private static void Throw() => throw new InvalidOperationException("expected failure");

        private sealed class RecordingOutput : IConsoleOutput
        {
            public List<string> Messages { get; } = new List<string>();
            public List<LogLevel?> Levels { get; } = new List<LogLevel?>();

            public void AppendToOutput(string message, LogLevel? level = null)
            {
                Messages.Add(message);
                Levels.Add(level);
            }
        }
    }
}
