# GitAutoUpdater

A lightweight .NET console application that keeps a cloned Git repository synchronized with a remote branch. It automatically clones the repository when the local copy is missing, checks for remote changes at a configurable interval, and pulls updates when they are available.

[Español](README.es.md)

## Features

- Clone a repository automatically when the local copy does not exist or is invalid.
- Monitor a configured remote branch for changes.
- Pull updates automatically when the local and remote commits differ.
- Configure the polling interval, branch, local path, and log file through `settings.json`.
- Write color-coded status messages to the console and detailed entries to a log file.
- Detect and report common Git errors, including authentication, network, repository, and branch errors.
- Cancel an active Git operation by pressing `Q`.
- Clean up active Git processes when the application exits.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Git](https://git-scm.com/downloads) installed and available in the system `PATH`.
- Read access to the remote repository. Private repositories may require Git credentials or a configured credential helper.

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
| `UseDedicatedFolder` | When `true`, creates a subdirectory named after the repository inside `LocalPath`. |
| `IntSeconds` | Seconds between update checks. The value must be greater than 10. |
| `LogFile` | Log filename. Logs are stored in the `Logs` directory. |

> The initial `settings.json` contains placeholders. Replace `RepoUrl` and review the remaining values before running the updater.

## Build and run

Clone the repository and switch to the CLI branch:

```bash
git clone https://github.com/RafaxWolf/GitAutoUpdater.git
cd GitAutoUpdater
git checkout CLI
```

Restore dependencies, build, and run the application:

```bash
dotnet restore
dotnet build --configuration Release
dotnet run --configuration Release
```

To run the compiled application:

```bash
dotnet ./bin/Release/net10.0/GitAutoUpdater.dll
```

The application creates the configured local repository directory and the `Logs` directory relative to its runtime directory.

## Controls

- Press `Q` to request cancellation of the current Git operation.
- Press `Ctrl+C` to stop the application.

## How it works

1. Loads and validates `settings.json`.
2. Verifies that Git is installed and accessible.
3. Resolves the local repository path.
4. Clones the configured branch if the local directory is not a valid Git repository.
5. Fetches remote references at the configured interval.
6. Compares the local `HEAD` with `origin/<Branch>`.
7. Runs `git pull` when a difference is detected.
8. Records activity and errors in the console and log file.

## Project structure

- `App.cs` — Application entry point and update loop.
- `Core/` — Configuration, logging, timing, and Git-related services.
- `Schemas/` — Configuration models.
- `settings.json` — Runtime configuration template.
- `GitAutoUpdater.csproj` — .NET project definition.

## Limitations and notes

- The updater is designed for a working-tree synchronization workflow and does not resolve local uncommitted changes or merge conflicts automatically.
- Git credentials are handled by Git and are not stored by this application.
- The application runs continuously until it is interrupted.

## License

This project is distributed under the [MIT License](LICENSE.txt).
