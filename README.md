# VaultPublisher

[![.NET](https://github.com/chasebussey/VaultPublisher/actions/workflows/dotnet.yml/badge.svg)](https://github.com/chasebussey/VaultPublisher/actions/workflows/dotnet.yml)

VaultPublisher is a .NET utility to assist in publishing markdown files from an Obsidian vault to
a static site generator (currently built with [Quartz](https://quartz.jzhao.xyz) in mind). The tool will search an Obsidian vault
for any documents with `publish: true` in the frontmatter and copy them to designated destination directory.

# Installation
## Build from Source
1. Prerequisites:
   - .NET 10 SDK
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
- `--no-delete`: Do not delete destination files.
- `--show-published`: Include the published files in the output.
- `--preview`: Show planned copies and deletions without creating directories or changing files.
- `--exclude-dirs`: Exclude a directory name at any depth (for example, `--exclude-dirs private`). Repeat the option for additional names.

VaultPublisher recursively scans the source directory for Markdown files containing `publish: true` in the frontmatter. Published files retain their paths relative to the source root:

```text
vault/projects/research/note.md → site-content/projects/research/note.md
```

Files with the same name in different directories remain separate. Changed destination files are overwritten; unchanged files are skipped. Directories are created only when needed for a copied file. Empty source directories are not copied, and existing empty destination directories are left alone. Non-Markdown files are neither copied nor deleted.

Excluded directory names apply at every depth and prevent traversal of their entire subtrees. In saved configuration, `excludeDirs` is a comma-separated string, such as `"private,drafts"`.

:warning: **Warning**: Use a destination dedicated to published notes. By default, destination Markdown files are deleted if their exact relative paths are absent from the publishable source set. This includes notes removed from the source, marked unpublished, or placed under an excluded directory. Use `--no-delete` to preserve these files.

The source must exist. The destination may be missing, including during preview. Source and destination must not be the same directory or nested inside each other. Selected roots and entries encountered within either tree must not be symbolic links or directory junctions. Parent-directory aliases are resolved before checking whether the roots overlap.

Validation and scanning finish before any destination changes begin. A scanning error (including an invalid `publish` value) aborts publication. Filesystem errors are reported with a nonzero exit code. Execution is not transactional: a failure during copying or deletion can leave earlier operations completed. Avoid changing either tree concurrently while publishing.

Actual publication to your site is not handled by this tool. You will need to run your static site generator to build the site after the files are copied.

## Configuration
VaultPublisher allows for user configuration to be stored in a `config.json` file at `~/.vaultpublisher/config.json`. The configuration file is optional. It is a JSON file with the following structure:
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