using System.Reflection;
using MLGWorks.DevConsole.Runtime.Commands;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.CoreTests
{
    public class CommandInfoTests
    {
        [Test]
        public void ConstructorStoresMetadata()
        {
            var method = typeof(CommandInfoTests).GetMethod(nameof(NoOp), BindingFlags.Static | BindingFlags.NonPublic);
            var command = new CommandInfo("test", "description", method, new[] { "t" });

            Assert.That(command.Name, Is.EqualTo("test"));
            Assert.That(command.Description, Is.EqualTo("description"));
            Assert.That(command.Method, Is.EqualTo(method));
            Assert.That(command.Aliases, Is.EqualTo(new[] { "t" }));
        }

        [Test]
        public void MissingAliasesBecomeEmptyArray()
        {
            var command = new CommandInfo("test", "description", typeof(CommandInfoTests)
                .GetMethod(nameof(NoOp), BindingFlags.Static | BindingFlags.NonPublic));

            Assert.That(command.Aliases, Is.Empty);
        }

        [Test]
        public void CommandAttributeSupportsDangerLevel()
        {
            var method = typeof(CommandInfoTests).GetMethod(nameof(DangerousCommand), BindingFlags.Static | BindingFlags.NonPublic);
            var attribute = method.GetCustomAttribute<CommandAttribute>();

            Assert.That(attribute.DangerLevel, Is.EqualTo(CommandDangerLevel.Dangerous));
            Assert.That(attribute.EnabledByDefault, Is.False);
        }

        [Test]
        public void CommandSchemeUsesRequiredParameterMarkers()
        {
            var command = Create(nameof(Required));
            Assert.That(command.GetCommandScheme(), Is.EqualTo("required <int amount>"));
        }

        [Test]
        public void CommandSchemeUsesOptionalParameterMarkers()
        {
            var command = Create(nameof(Optional));
            Assert.That(command.GetCommandScheme(), Is.EqualTo("optional [int amount]"));
        }

        [Test]
        public void UsageContainsCommandScheme()
        {
            var command = Create(nameof(Optional));
            Assert.That(command.GetUsage(), Is.EqualTo("Usage: optional [int amount]"));
        }

        [Test]
        public void HelpContainsAliasesAndDescription()
        {
            var method = typeof(CommandInfoTests).GetMethod(nameof(Optional), BindingFlags.Static | BindingFlags.NonPublic);
            var command = new CommandInfo("optional", "A test command", method, new[] { "opt" });

            StringAssert.Contains("optional (aliases: opt)", command.GetHelp());
            StringAssert.Contains("A test command", command.GetHelp());
        }

        [Test]
        public void HelpWithoutAliasesOmitsAliasSection()
        {
            var command = Create(nameof(NoOp));
            Assert.That(command.GetHelp(), Does.Not.Contain("aliases:"));
        }

        private static CommandInfo Create(string methodName)
        {
            var method = typeof(CommandInfoTests).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
            return new CommandInfo(methodName.ToLowerInvariant(), "description", method);
        }

        private static void NoOp() { }
        private static void Required(int amount) { }
        private static void Optional(int amount = 1) { }

        [Command("dangerous", DangerLevel = CommandDangerLevel.Dangerous, EnabledByDefault = false)]
        private static void DangerousCommand() { }
    }
}
