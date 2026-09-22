# GitAutoUpdater

A cross-platform desktop application for keeping a cloned Git repository synchronized with a remote branch. The `Desktop` branch is the graphical-user-interface edition of GitAutoUpdater and is intended to use [Avalonia UI](https://avaloniaui.net/) for a native-feeling desktop experience across Windows, Linux, and macOS.

***Language***
- 🇺🇸 English
- [🇪🇸 Español](README.es.md)

> **Development status:** The Desktop branch is being prepared for the Avalonia UI migration. The repository synchronization services and configuration model are the foundation for the desktop interface; Avalonia views, view models, and platform packaging may still be under active development.

## Features

- Automatically clone a repository when the local copy is missing or invalid.
- Monitor a selected remote branch for new commits.
- Fetch remote references and pull changes when updates are available.
- Configure the repository URL, branch, local path, polling interval, and log file through `settings.json`.
- Display synchronization status, progress, warnings, and errors through a desktop UI as the Avalonia migration progresses.
- Keep detailed activity logs in the `Logs` directory.
- Detect common Git failures, including authentication, network, repository, and branch errors.
- Cancel active Git operations safely and clean up Git processes when the application closes.

## Technology

- **C# / .NET 10**
- **Avalonia UI** for the cross-platform desktop interface
- **Git CLI** for cloning, fetching, comparing revisions, and pulling updates
- **JSON configuration** through `settings.json`

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Git](https://git-scm.com/downloads) installed and available in the system `PATH`
- Read access to the remote repository
- An operating system supported by Avalonia UI, such as Windows, Linux, or macOS

Private repositories may require Git credentials or a configured Git credential helper. GitAutoUpdater does not store repository credentials.

## Configuration

Edit `settings.json` before starting the application:

```json
{
  "AppSettings": {
    "GitSettings": {
      "RepoUrl": "https://github.com/owner/repository.git",
      "Branch": "main",
      "LocalPath": "Default",
      "UseDedicatedFolder": false
    },
    "IntSeconds": 60,
    "LogFile": "logs.log"
  }
}
```

### Settings reference

| Property | Description |
| --- | --- |
| `RepoUrl` | URL of the Git repository to clone and monitor. |
| `Branch` | Remote branch to track, such as `main` or `develop`. |
| `LocalPath` | Destination path relative to the application directory. `Default` and `./` use the `Repository` directory. |
| `UseDedicatedFolder` | When `true`, creates a repository-named subdirectory inside `LocalPath`. |
| `IntSeconds` | Seconds between update checks. The value must be greater than 10. |
| `LogFile` | Log filename. Logs are stored in the `Logs` directory. |

> The default configuration contains an empty `RepoUrl`. Replace it and review the remaining values before running the application.

## Build and run

Clone the repository and switch to the Desktop branch:

```bash
git clone https://github.com/RafaxWolf/GitAutoUpdater.git
cd GitAutoUpdater
git checkout Desktop
```

Restore dependencies and build the project:

```bash
dotnet restore
dotnet build --configuration Release
```

Run the desktop application with:

```bash
dotnet run --configuration Release
```

When Avalonia platform projects and packaging targets are added, platform-specific publish commands will be documented here. For now, use the project configuration present in this branch as the source of truth.

## Desktop UI direction

The Avalonia UI is intended to provide:

- A repository configuration screen for editing and validating `settings.json` values.
- A dashboard showing the tracked repository, branch, local path, current state, and last check.
- Start, pause, update, and cancel controls for synchronization operations.
- A live activity area for Git output and user-friendly error messages.
- A log viewer or shortcut to the generated log file.
- A clean shutdown flow that cancels active Git processes before the window closes.

The synchronization logic should remain separated from the presentation layer so that Git operations can run asynchronously without blocking the Avalonia UI thread.

## How synchronization works

1. Loads and validates `settings.json`.
2. Verifies that Git is installed and accessible.
3. Resolves the local repository path.
4. Clones the configured branch if the local directory is not a valid Git repository.
5. Fetches remote references at the configured interval.
6. Compares the local `HEAD` with `origin/<Branch>`.
7. Pulls the branch when a difference is detected.
8. Reports progress and errors to the desktop UI and the log file.

## Project structure

- `App.cs` — Current application entry point and synchronization loop.
- `Core/` — Configuration, logging, timing, and Git services.
- `Schemas/` — Configuration models.
- `settings.json` — Runtime configuration template.
- `GitAutoUpdater.csproj` — .NET project definition and dependencies.
- `GitAutoUpdater.slnx` — Solution file.

As the Avalonia migration advances, UI-specific folders such as `Views/`, `ViewModels/`, `Assets/`, and platform configuration files may be added to this structure.

## Limitations and notes

- The Desktop branch is under active development and its Avalonia UI is not necessarily complete yet.
- Local uncommitted changes and merge conflicts are not resolved automatically.
- Git credentials are handled by Git and are not stored by this application.
- The application is designed to keep running until the user pauses or closes it.

## License

This project is distributed under the [MIT License](LICENSE.txt).
