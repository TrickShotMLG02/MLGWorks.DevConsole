using System.Collections.Generic;
using System.Reflection;
using MLGWorks.DevConsole.Runtime.Abstractions;
using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Core;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.CoreTests
{
    public class AutocompleteEngineTests
    {
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

        private sealed class TestRegistry : ICommandRegistry
        {
            public TestRegistry(CommandInfo command)
            {
                Commands = new Dictionary<string, CommandInfo>
                {
                    [command.Name] = command
                };
            }

            public IReadOnlyDictionary<string, CommandInfo> Commands { get; }
            public IEnumerable<CommandInfo> CommandInfos => Commands.Values;

            public void RegisterAll()
            {
            }

            public bool UnregisterCommand(string name) => false;
        }
    }
}
