# VaultPublisher

VaultPublisher is a .NET Core utility to assist in publishing markdown files from an Obsidian vault to
a static site generator (currently built with [Quartz](https://quartz.jzhao.xyz) in mind). The tool will search an Obsidian vault
for any documents with `publish: true` in the frontmatter and copy them to designated destination directory.

# Installation
## Build from Source
1. Prerequisites:
   - .NET Core 9.0
2. Steps:
   ```sh
   # Clone the repository
   git clone https://github.com/chasebussey/VaultPublisher.git

   # Navigate to the project directory
   cd VaultPublisher

   # Install dependencies
   dotnet restore
   
   # Build the project
   dotnet build
   ```

# Usage
## Publishing
`vaultpublisher publish --source <source> --destination <destination> --verbose`

The `publish` command supports the following options:
- `--source` or `-s`: The source directory to scan for Markdown files to publish.
- `--destination` or `-d`: The destination directory to copy the published files to.
- `--verbose` or `-v`: Enable verbose output.
- `--no-delete` or `-n`: Do not delete files in the destination directory that are not marked for publication in the source directory.

VaultPublisher scans the source directory for Markdown files containing `publish: true` in the frontmatter. If a file is found, it will be copied to the destination directory. If the file already exists in the destination directory, it will be overwritten.

:warning: **Warning**: Currently, VaultPublisher does not support nested directories. This is a high-priority item to be corrected before this tool is release on NuGet.

:warning: **Warning**: This tool will delete files in the destination directory that are not marked for publication in the source directory unless the --no-delete flag is set.

Actual publication to your site is not handled by this tool. You will need to run your static site generator to build the site after the files are copied.

## Configuration
VaultPublisher allows for user configuration to be stored in a `config.json` file at `~/.config/vaultpublisher/config.json`. The configuration file is optional. It is a JSON file with the following structure:
```json
{
  "source": "<source>",
  "destination": "<destination>",
  "noDelete": false
}
```

Any values not present in the configuration file will be taken from the command line arguments, and if a config and command line argument are both present, the command line argument will take precedence.

The `config` command has the following subcommands:
- `get`: Get the current configuration by key. If no key is provided, all settings will be returned.
- `set`: Set a configuration value by key.
- `remove`: Remove a configuration value by key.

# TODO
- [x] Allow user configuration/persistent settings.
- [ ] Add support for nested directories.
- [ ] Improve/add output.
- [ ] Interactive execution?