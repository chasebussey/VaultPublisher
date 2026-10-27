# Agent guide

## Project overview

VaultPublisher is a command-line tool that copies Markdown notes marked
`publish: true` in Obsidian frontmatter to a static-site content directory, currently
with Quartz in mind. It copies content; it does not build or deploy the site.

## Repository map

- `VaultPublisher/Program.cs`: application entry point and command registration.
- `VaultPublisher/Commands/`: command definitions and handlers for publishing and configuration.
- `VaultPublisher/ConfigurationProvider.cs`: configuration loading, conversion, and persistence.
- `VaultPublisher.Tests/`: xUnit tests for publishing, configuration, and command handling.
- `.github/workflows/dotnet.yml`: CI restore, build, and test steps.
- `README.md`: installation, CLI usage, and configuration documentation.

## Build and validation

Use the .NET 10 SDK. Run these commands from the repository root, matching CI:

```sh
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

For a focused test run:

```sh
dotnet test --filter FullyQualifiedName~PublishCommandHandlerTests
```

The application enables nullable reference types and treats warnings as errors.
Report which checks were run and any validation limitations; do not claim tests
passed if the SDK is unavailable.

## Safety and behavior

- Publishing can overwrite files and deletes unpublished destination Markdown by
  default. Never test against a real vault or site directory; use disposable fixtures.
- Preserve the intended safety contracts of `--preview` (no filesystem mutations)
  and `--no-delete` (no destination deletions). Add regression tests when changing
  copying, deletion, traversal, exclusion, or path handling.
- Use unique temporary directories for new filesystem tests and clean them up.
  Avoid reading or writing the developer's actual configuration in tests.
- CLI arguments are intended to override saved configuration. Cover precedence
  changes with tests.
- Configuration lives at `~/.vaultpublisher/config.json`. Preserve this storage
  location unless explicitly migrating it.

## Implementation conventions

- Follow the surrounding C# conventions and keep changes focused on the task.
- Separate command wiring, publishing behavior, and configuration persistence.
- Use blank lines between semantic phases, keep calculations with their guards,
  and prefer named intermediate values over deeply nested expressions.
- Add or update xUnit tests alongside behavior changes, including relevant failure
  paths. Avoid new dependencies unless the task warrants them.
- Update `README.md` for user-visible CLI or configuration changes. Record notable
  release changes in `VaultPublisher/CHANGELOG.md` when appropriate.
- Do not commit build outputs, NuGet packages, local IDE settings, or user config.
