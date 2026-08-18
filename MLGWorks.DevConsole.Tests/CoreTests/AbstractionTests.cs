using System.Linq;
using MLGWorks.DevConsole.Runtime.Abstractions;
using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.Utils.Logging;
using NUnit.Framework;

namespace MLGWorks.DevConsole.Tests.CoreTests
{
    public class AbstractionTests
    {
        [Test]
        public void RegistryExposesRegisteredCommands()
        {
            CommandManager.RegisterAll();
            var registry = new CommandManagerRegistry();

            Assert.That(registry.Commands.ContainsKey("help"), Is.True);
            Assert.That(registry.CommandInfos.Any(command => command.Name == "help"), Is.True);
        }

        [Test]
        public void RegistryCanRegisterAllAndUnregister()
        {
            var registry = new CommandManagerRegistry();
            registry.RegisterAll();

            Assert.That(registry.UnregisterCommand("help"), Is.True);
            Assert.That(registry.Commands.ContainsKey("help"), Is.False);
        }

        [Test]
        public void RegistryUnregisterMissingCommandReturnsFalse()
        {
            var registry = new CommandManagerRegistry();
            Assert.That(registry.UnregisterCommand("missing"), Is.False);
        }

        [Test]
        public void ExecutorDelegatesToCommandManager()
        {
            CommandManager.RegisterAll();
            var output = new RecordingOutput();
            var executor = new CommandManagerExecutor(output);

            Assert.That(executor.TryExecute("time", out var result), Is.True);
            Assert.That(result, Does.Match("^\\d{4}-\\d{2}-\\d{2} "));
            Assert.That(output.Messages.Single(), Is.EqualTo("> time"));
        }

        [Test]
        public void ExecutorWorksWithoutOutput()
        {
            CommandManager.RegisterAll();
            var executor = new CommandManagerExecutor();

            Assert.That(executor.TryExecute("time", out var result), Is.True);
            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public void InputFactoryCreatesProductionSource()
        {
            var source = new ConsoleInputFactory().Create();

            Assert.That(source, Is.TypeOf<ConsoleInputSource>());
            source.Dispose();
        }

        [Test]
        public void InputSourceExposesHistoryBindings()
        {
            using (var source = new ConsoleInputSource())
            {
                Assert.That(source.HistoryBindingPaths, Is.Not.Null);
                Assert.That(source.HistoryBindingPaths.Count, Is.GreaterThanOrEqualTo(2));
            }
        }

        [Test]
        public void InputSourceCanBeEnabledAndDisabledRepeatedly()
        {
            using (var source = new ConsoleInputSource())
            {
                Assert.DoesNotThrow(() => source.Enable());
                Assert.DoesNotThrow(() => source.Enable());
                Assert.DoesNotThrow(() => source.Disable());
                Assert.DoesNotThrow(() => source.Disable());
            }
        }

        [Test]
        public void InputSourceDisposeIsIdempotent()
        {
            var source = new ConsoleInputSource();

            Assert.DoesNotThrow(() => source.Dispose());
            Assert.DoesNotThrow(() => source.Dispose());
        }

        [Test]
        public void InputSourceCannotBeEnabledAfterDispose()
        {
            var source = new ConsoleInputSource();
            source.Dispose();

            Assert.Throws<System.ObjectDisposedException>(() => source.Enable());
        }

        private sealed class RecordingOutput : IConsoleOutput
        {
            public System.Collections.Generic.List<string> Messages { get; } = new();

            public void AppendToOutput(string message, LogLevel? level = null)
            {
                Messages.Add(message);
            }
        }
    }
}
