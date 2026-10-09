# QuickDecompile

QuickDecompile is a lightweight, fast, and simple desktop application designed to view decompiled code instantly. It acts like a "Notepad for decompilation" — no complex project setups, no settings menus, just right-click a compiled file and see the code.

## Supported Formats
- **.NET**: `.dll`, `.exe` (decompiled using ICSharpCode.Decompiler)
- **Java**: `.class`, `.jar`, `.war` (decompiled using Vineflower)
- **Android**: `.dex`, `.apk`, `.aab` (decompiled using jadx)

## Architecture
- **Core**: File format detection and caching models.
- **Backends**: Wrappers for underlying decompilation tools.
- **Shell**: Integration with the Windows Context Menu and single-instance IPC.
- **UI**: Avalonia UI application using AvaloniaEdit for syntax highlighting.

## Getting Started

### Prerequisites
- .NET 8.0 SDK (for building)
- Java Runtime Environment (JRE) installed and in your `PATH` or `JAVA_HOME` (required for Java and Android decompilation)

### External Decompilers
QuickDecompile relies on external tools to decompile Java and Android applications. You must download these JAR files and place them in the same folder as the `QuickDecompile.UI.exe` binary.

1. **Vineflower** (for Java `.class` and `.jar`):
   - Download the latest `vineflower-X.Y.Z.jar` from the [Vineflower Releases](https://github.com/Vineflower/vineflower/releases) page.
   - Rename it to `vineflower.jar` and place it next to the executable.

2. **jadx** (for Android `.apk` and `.dex`):
   - Download the latest `jadx-X.Y.Z.zip` from the [jadx Releases](https://github.com/skylot/jadx/releases) page.
   - Extract `jadx-cli.jar` and place it next to the executable as `jadx.jar`.

*(Alternatively, QuickDecompile will attempt to search for any `*vineflower*.jar`, `*cfr*.jar`, or `*jadx*.jar` in the application's root directory).*

### Build and Run
```bash
dotnet build QuickDecompile.sln
dotnet run --project src/QuickDecompile.UI
```

To test passing a file directly:
```bash
dotnet run --project src/QuickDecompile.UI -- "path/to/some/file.dll"
```

## Features
- **Context Menu Integration**: Add "Open with QuickDecompile" directly to your Windows context menu from the application settings.
- **Instant Viewing**: Fast launch times and in-memory caching.
- **Tabbed Interface**: Open multiple files seamlessly.
- **Tree Viewer**: Browse container structures (JARs, APKs, DLLs) efficiently.

## Third-Party Licenses

This project integrates or depends on the following excellent open-source projects:

- **[Avalonia UI](https://github.com/AvaloniaUI/Avalonia)**: Licensed under the MIT License.
- **[AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)**: Licensed under the MIT License.
- **[ICSharpCode.Decompiler](https://github.com/icsharpcode/ILSpy)**: Licensed under the MIT License.
- **[Vineflower](https://github.com/Vineflower/vineflower)**: Licensed under the GPL-2.0 License.
- **[CFR](https://github.com/leibnitz27/cfr)**: Licensed under the MIT License.
- **[jadx](https://github.com/skylot/jadx)**: Licensed under the Apache-2.0 License.

Please ensure you comply with the respective licenses when using or distributing this software.

## MVP Scope

Currently, this tool is strictly a viewer. Out of scope for this initial MVP:
- Recompilation and editing
- Deobfuscation and patching
- Dynamic analysis / debugging
- Heavy plugins
