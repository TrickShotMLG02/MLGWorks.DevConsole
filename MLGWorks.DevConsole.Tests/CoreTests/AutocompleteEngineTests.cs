using System.Collections.Generic;
using System.Reflection;
using MLGWorks.DevConsole.Runtime.Abstractions;
using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Core;
using MLGWorks.DevConsole.Runtime.Configuration;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.CoreTests
{
    public class AutocompleteEngineTests
    {
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void EmptyInputHasNoSuggestion(string input)
        {
            var autocomplete = new AutocompleteEngine(new TestRegistry());
            Assert.That(autocomplete.GetSuggestion(input), Is.Empty);
        }

        [Test]
        public void MatchingIsCaseInsensitive()
        {
            var command = CreateCommand("help", nameof(TestCommand));
            var autocomplete = new AutocompleteEngine(new TestRegistry(command));

            Assert.That(autocomplete.GetSuggestion("HE"), Is.EqualTo("HElp"));
        }

        [Test]
        public void UnknownCommandHasNoSuggestionAndNoMatch()
        {
            var autocomplete = new AutocompleteEngine(new TestRegistry(CreateCommand("help", nameof(TestCommand))));

            var suggestion = autocomplete.GetSuggestion("xyz", out var command);

            Assert.That(suggestion, Is.Empty);
            Assert.That(command, Is.Null);
        }

        [Test]
        public void SuggestionPreservesTypedArguments()
        {
            var command = CreateCommand("move", nameof(CommandWithArguments));
            var autocomplete = new AutocompleteEngine(new TestRegistry(command));

            Assert.That(autocomplete.GetSuggestion("mo 10"), Is.EqualTo("move 10 <name: string>"));
        }

        [Test]
        public void SuggestionShowsOptionalArguments()
        {
            var command = CreateCommand("optional", nameof(CommandWithOptionalArgument));
            var autocomplete = new AutocompleteEngine(new TestRegistry(command));

            Assert.That(autocomplete.GetSuggestion("opt"), Is.EqualTo("optional [value: int]"));
        }

        [Test]
        public void SuggestionShowsParamsArguments()
        {
            var command = CreateCommand("many", nameof(CommandWithParams));
            var autocomplete = new AutocompleteEngine(new TestRegistry(command));

            Assert.That(autocomplete.GetSuggestion("ma"), Is.EqualTo("many <string[]>"));
            Assert.That(autocomplete.GetSuggestion("many one"), Is.EqualTo("many one"));
        }

        [Test]
        public void AutocompleteRequestIsConsumedOnce()
        {
            var command = CreateCommand("help", nameof(TestCommand));
            var autocomplete = new AutocompleteEngine(new TestRegistry(command));
            autocomplete.GetSuggestion("he");
            autocomplete.SetMatchedCommand(command);

            autocomplete.RequestAutoComplete();
            Assert.That(autocomplete.TryPerformAutoComplete("he"), Is.EqualTo("help"));
            Assert.That(autocomplete.TryPerformAutoComplete("he"), Is.Null);
        }

        [Test]
        public void AutocompleteDoesNothingForExactCommand()
        {
            var command = CreateCommand("help", nameof(TestCommand));
            var autocomplete = new AutocompleteEngine(new TestRegistry(command));
            autocomplete.GetSuggestion("help");
            autocomplete.SetMatchedCommand(command);
            autocomplete.RequestAutoComplete();

            Assert.That(autocomplete.TryPerformAutoComplete("help"), Is.Null);
        }

        [Test]
        public void AutocompleteDoesNothingWhenInputAlreadyHasArguments()
        {
            var command = CreateCommand("help", nameof(TestCommand));
            var autocomplete = new AutocompleteEngine(new TestRegistry(command));
            autocomplete.GetSuggestion("he");
            autocomplete.SetMatchedCommand(command);
            autocomplete.RequestAutoComplete();

            Assert.That(autocomplete.TryPerformAutoComplete("help arg"), Is.Null);
        }

        [Test]
        public void ClearingMatchedCommandDisablesPendingCompletion()
        {
            var command = CreateCommand("help", nameof(TestCommand));
            var autocomplete = new AutocompleteEngine(new TestRegistry(command));
            autocomplete.GetSuggestion("he");
            autocomplete.SetMatchedCommand(command);
            autocomplete.RequestAutoComplete();
            autocomplete.SetMatchedCommand(null);

            Assert.That(autocomplete.TryPerformAutoComplete("he"), Is.Null);
        }

        [Test]
        public void SuggestionUsesTheInjectedRegistry()
        {
            var command = new CommandInfo(
                "help",
                "Shows help",
                typeof(AutocompleteEngineTests).GetMethod(nameof(TestCommand), BindingFlags.Static | BindingFlags.NonPublic),
                new[] { "h" });
            var registry = new TestRegistry(command);
            var autocomplete = new AutocompleteEngine(registry);

            var suggestion = autocomplete.GetSuggestion("he", out var matchedCommand);

            Assert.That(suggestion, Is.EqualTo("help"));
            Assert.That(matchedCommand, Is.SameAs(command));
        }

        private static void TestCommand()
        {
        }

        private static void CommandWithArguments(int amount, string name)
        {
        }

        private static void CommandWithOptionalArgument(int value = 1)
        {
        }

        private static void CommandWithParams(params string[] values)
        {
        }

        private static CommandInfo CreateCommand(string name, string methodName)
        {
            return new CommandInfo(name, "Test command", typeof(AutocompleteEngineTests)
                .GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic));
        }

        private sealed class TestRegistry : ICommandRegistry
        {
            public TestRegistry(CommandInfo command = null)
            {
                Commands = command == null
                    ? new Dictionary<string, CommandInfo>()
                    : new Dictionary<string, CommandInfo> { [command.Name] = command };
            }

            public IReadOnlyDictionary<string, CommandInfo> Commands { get; }
            public IEnumerable<CommandInfo> CommandInfos => Commands.Values;

            public void RegisterAll()
            {
            }

            public void RegisterFromSettings(DevConsoleCommandSettings settings)
            {
            }

            public bool UnregisterCommand(string name) => false;
        }
    }
}
