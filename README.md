# VaultPublisher

VaultPublisher is a .NET Core utility to assist in publishing markdown files from an Obsidian vault to
a static site generated with [Quartz](https://quartz.jzhao.xyz). The tool will search an Obsidian vault
for any documents with `publish: true` in the frontmatter and copy them to the Quartz content directory.

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
## Install as dotnet tool
`dotnet tool install --global VaultPublisher`

# Usage
Installing or building the tool from source will create a `vaultpublisher` executable. To use the tool, run the executable with the following arguments:
- `--source` or `-s`: The path to the Obsidian vault directory
- `--destination` or `-d`: The path to the Quartz content directory
- `--verbose` or `-v`: (Default: false) Enable verbose logging
- `--no-delete`: (Default: false) Do not delete files in the destination directory that are not marked for publication in the source directory

Note that the default behavior is to delete files in the destination directory that are not marked for publication in the source directory.

VaultPublisher scans the source directory for Markdown files containing `publish: true` in the frontmatter. If a file is found, it will be copied to the destination directory. If the file already exists in the destination directory, it will be overwritten.

Actual publication to your Quartz site is not handled by this tool. You will need to run the Quartz build command to generate the site after running VaultPublisher.

# TODO
- [ ] Allow user configuration/persistent settings
- [ ] Improve/add output
- [ ] Interactive execution