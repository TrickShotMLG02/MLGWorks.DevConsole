# MLGWorks DevConsole 🛠️

[![Unity](https://img.shields.io/badge/Unity-6000.0%2B-black?logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/tests-editor%20%2B%20playmode-success.svg)](MLGWorks.DevConsole.Tests/)

A modular, extensible developer console for Unity with command discovery, autocomplete, history, runtime logging, and an editor-managed command catalog.

## 📦 Install through Unity Package Manager

In Unity, open **Window → Package Manager**, select **Add package from git URL**, and enter:

```text
https://github.com/TrickShotMLG02/MLGWorks.DevConsole.git
```

The repository root is a UPM package named `com.mlgworks.devconsole` and automatically installs its `MLGWorks.Utils` dependency.

## ✨ Highlights

- 🧩 Modular runtime services with interfaces for input, output, history, autocomplete, execution, and registration.
- 🔎 Editor-time command discovery with stable catalog entries.
- 🎛️ Per-command and per-class enable/disable controls.
- 🧪 Test-only command detection that keeps test commands out of runtime registration.
- ⚠️ Warning and dangerous command highlighting.
- 🔒 Fail-closed startup when the command catalog is missing.
- 🧠 Command history, autocomplete, aliases, and scrollback management.
- 📚 Built-in math, variable, execution, help, and console commands.

## 🚀 Quick start

1. Open the project in Unity 6 or install the package in a Unity 6 project.
2. Ensure the `DevConsole` prefab is present in your scene.
3. Open **Window → MLGWorks → DevConsole Commands**.
4. Create or select a `DevConsoleCommandSettings` asset.
5. Click **Refresh Discovery**.
6. Import the **DevConsole Sample** through Package Manager. It contains a configured scene and command catalog.
7. Enter Play Mode and open the console using the configured input action.

The catalog is the runtime source of truth. If no catalog can be found, no commands are registered.

## 📖 Documentation

The complete setup and configuration guide is available in the [LaTeX documentation](Documentation/DevConsoleDocumentation.pdf) and its [source](Documentation/DevConsoleDocumentation.tex).

The ready-to-import demonstration command is included in the `DevConsole Sample` package sample under `Samples~/DevConsoleSample`.

## ⌨️ Defining commands

Commands are static methods decorated with `CommandAttribute`:

```csharp
using MLGWorks.DevConsole.Runtime.Commands;

public static class GameCommands
{
    [Command("weather", "Changes the current weather", "w")]
    public static string Weather(string value) => $"Weather: {value}";

    [Command( "reset-save", "Deletes the current save data", DangerLevel = CommandDangerLevel.Dangerous, EnabledByDefault = false)]
    public static string ResetSave() => "Save reset.";
}
```

`EnabledByDefault` affects newly discovered catalog entries only. Refreshing discovery preserves existing enable/disable choices.

## 🔍 Catalog search

The command window supports focused search modes:

- No prefix: command names only.
- `$`: declaring classes and namespaces.
- `#`: all catalog metadata, including command, class, assembly, and method names.

## ⚠️ Safety model

`CommandDangerLevel.Warning` and `CommandDangerLevel.Dangerous` change editor presentation and make risky commands visible during review. They do not replace authorization or confirmation in a shipped product. Dangerous commands should remain disabled unless explicitly required, and applications should add their own confirmation or access policy where appropriate.

## 🧪 Tests

The repository contains editor and playmode tests covering core services, built-in commands, math commands, catalog behavior, input lifecycle, stale entries, and regression cases. Run them through Unity Test Runner before shipping changes.

## 📄 License

MLGWorks DevConsole is released under the [MIT License](LICENSE).

Maintained by [TrickShotMLG02](https://github.com/TrickShotMLG02).
