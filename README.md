# ADOLogger

ADOLogger is a lightweight .NET library for writing Azure DevOps logging commands.

It provides helpers for:

- Formatting commands (group, warning, error, section, debug, command)
- Task commands (log issue, set variable, progress, endpoints, attachments)
- Artifact commands (associate and upload)
- Build and release commands

## Installation

```bash
dotnet add package ADOLogger
```

## Quick Start

```csharp
using ADOLogger;

var logger = new ADOLogger();
logger.Info("Starting build step");
logger.Warning("Potential issue detected");
logger.Error("Build failed");
```
