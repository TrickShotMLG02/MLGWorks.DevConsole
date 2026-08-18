using System;
using MLGWorks.DevConsole.Runtime.Commands;
using MLGWorks.DevConsole.Runtime.Configuration;
using NUnit.Framework;
using UnityEngine;

namespace MLGWorks.DevConsole.Tests.EditorTests
{
    public class CommandCatalogEditorTests
    {
        private DevConsoleCommandSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<DevConsoleCommandSettings>();
            CommandManager.Output = null;
        }

        [TearDown]
        public void TearDown()
        {
            CommandManager.Output = null;
            UnityEngine.Object.DestroyImmediate(_settings);
        }

        [Test]
        public void DisabledCommandIsNotRegistered()
        {
            _settings.ReplaceCommands(new[] { Definition(nameof(CatalogTestCommands.Visible), "catalog-visible", enabled: false) });

            CommandManager.RegisterFromSettings(_settings);

            Assert.That(CommandManager.Commands.ContainsKey("catalog-visible"), Is.False);
        }

        [Test]
        public void TestOnlyCommandIsDisabledFromCatalogButDirectTestsStillRun()
        {
            var command = Definition(nameof(CatalogTestCommands.Visible), "catalog-visible");
            command.isTestOnly = true;
            _settings.ReplaceCommands(new[] { command });

            CommandManager.RegisterFromSettings(_settings);

            Assert.That(CommandManager.Commands.ContainsKey("catalog-visible"), Is.False);
            Assert.That(_settings.GetCommandCount(), Is.EqualTo(0));
            Assert.That(_settings.GetDisabledCommandCount(), Is.EqualTo(0));
            Assert.That(CatalogTestCommands.Visible(), Is.EqualTo("visible"));
        }

        [Test]
        public void LegacyCatalogEntryFromTestAssemblyIsStillExcluded()
        {
            _settings.ReplaceCommands(new[] { Definition(nameof(CatalogTestCommands.Visible), "catalog-visible") });

            CommandManager.RegisterFromSettings(_settings);

            Assert.That(CommandManager.Commands.ContainsKey("catalog-visible"), Is.False);
        }

        [Test]
        public void DisabledClassExcludesAllCommandsFromThatClass()
        {
            _settings.ReplaceCommands(new[]
            {
                Definition(nameof(CatalogTestCommands.Visible), "catalog-visible"),
                Definition(nameof(CatalogTestCommands.Second), "catalog-second")
            });
            _settings.SetTypeDisabled(typeof(CatalogTestCommands).FullName, true);

            CommandManager.RegisterFromSettings(_settings);

            Assert.That(CommandManager.Commands.ContainsKey("catalog-visible"), Is.False);
            Assert.That(CommandManager.Commands.ContainsKey("catalog-second"), Is.False);
        }

        [Test]
        public void EnabledCatalogCommandCanExecute()
        {
            _settings.ReplaceCommands(new[] { Definition(nameof(CatalogTestCommands.Visible), "catalog-visible") });

            CommandManager.RegisterFromSettings(_settings);

            Assert.That(CommandManager.TryExecute("catalog-visible", out var result), Is.True);
            Assert.That(result, Is.EqualTo("visible"));
        }

        [Test]
        public void CatalogSkipsStaleMethodEntries()
        {
            _settings.ReplaceCommands(new[]
            {
                new DevConsoleCommandDefinition
                {
                    commandName = "catalog-visible",
                    description = "stale",
                    assemblyName = typeof(CatalogTestCommands).Assembly.GetName().Name,
                    declaringTypeName = typeof(CatalogTestCommands).FullName,
                    methodName = "MissingMethod",
                    parameterTypeNames = Array.Empty<string>(),
                    enabled = true
                }
            });

            CommandManager.RegisterFromSettings(_settings);

            Assert.That(CommandManager.Commands.ContainsKey("catalog-visible"), Is.False);
        }

        [Test]
        public void ReplacingCommandsPreservesEnablementByStableId()
        {
            var first = Definition(nameof(CatalogTestCommands.Visible), "catalog-visible");
            _settings.ReplaceCommands(new[] { first });
            _settings.SetCommandEnabled(first.StableId, false);

            _settings.ReplaceCommands(new[] { Definition(nameof(CatalogTestCommands.Visible), "catalog-visible") });

            Assert.That(_settings.Commands[0].enabled, Is.False);
        }

        [Test]
        public void RemovedCommandIsRetainedAsObsoleteButNotRegistered()
        {
            _settings.ReplaceCommands(new[] { Definition(nameof(CatalogTestCommands.Visible), "catalog-visible") });
            _settings.ReplaceCommands(Array.Empty<DevConsoleCommandDefinition>());

            Assert.That(_settings.Commands, Has.Count.EqualTo(1));
            Assert.That(_settings.Commands[0].isObsolete, Is.True);
            Assert.That(_settings.GetEnabledCommands(), Is.Empty);

            CommandManager.RegisterFromSettings(_settings);
            Assert.That(CommandManager.Commands.ContainsKey("catalog-visible"), Is.False);
        }

        [Test]
        public void ReappearingCommandRestoresPreviousEnablement()
        {
            var command = Definition(nameof(CatalogTestCommands.Visible), "catalog-visible");
            _settings.ReplaceCommands(new[] { command });
            _settings.SetCommandEnabled(command.StableId, false);
            _settings.ReplaceCommands(Array.Empty<DevConsoleCommandDefinition>());
            _settings.ReplaceCommands(new[] { Definition(nameof(CatalogTestCommands.Visible), "catalog-visible") });

            Assert.That(_settings.Commands, Has.Count.EqualTo(1));
            Assert.That(_settings.Commands[0].isObsolete, Is.False);
            Assert.That(_settings.Commands[0].enabled, Is.False);
        }

        [Test]
        public void ObsoleteCommandsCanBeRemovedExplicitly()
        {
            _settings.ReplaceCommands(new[] { Definition(nameof(CatalogTestCommands.Visible), "catalog-visible") });
            _settings.ReplaceCommands(Array.Empty<DevConsoleCommandDefinition>());

            Assert.That(_settings.RemoveObsoleteCommands(), Is.EqualTo(1));
            Assert.That(_settings.Commands, Is.Empty);
        }

        [Test]
        public void DisabledCommandMetricIncludesIndividualAndClassDisablesButExcludesObsolete()
        {
            var visible = Definition(nameof(CatalogTestCommands.Visible), "catalog-visible");
            var second = Definition(nameof(CatalogTestCommands.Second), "catalog-second");
            var obsolete = Definition(nameof(CatalogTestCommands.Obsolete), "catalog-obsolete");
            _settings.ReplaceCommands(new[] { visible, second, obsolete });
            _settings.SetCommandEnabled(visible.StableId, false);
            _settings.SetTypeDisabled(typeof(CatalogTestCommands).FullName, true);
            _settings.ReplaceCommands(new[] { visible, second });

            Assert.That(_settings.GetDisabledCommandCount(), Is.EqualTo(2));
        }

        private static DevConsoleCommandDefinition Definition(string methodName, string commandName, bool enabled = true)
        {
            var method = typeof(CatalogTestCommands).GetMethod(methodName);
            return new DevConsoleCommandDefinition
            {
                commandName = commandName,
                description = "Catalog test command",
                aliases = Array.Empty<string>(),
                assemblyName = typeof(CatalogTestCommands).Assembly.GetName().Name,
                declaringTypeName = typeof(CatalogTestCommands).FullName,
                methodName = methodName,
                parameterTypeNames = Array.ConvertAll(method.GetParameters(), parameter => parameter.ParameterType.AssemblyQualifiedName),
                enabled = enabled
            };
        }
    }

    public static class CatalogTestCommands
    {
        [Command("catalog-visible")]
        public static string Visible() => "visible";

        [Command("catalog-second")]
        public static string Second() => "second";

        [Command("catalog-obsolete")]
        public static string Obsolete() => "obsolete";
    }
}
