# QuartzPublisher

QuartzPublisher is a .NET Core utility to assist in publishing markdown files from an Obsidian vault to
a static site generated with [Quartz](https://quartz.jzhao.xyz). The tool will search an Obsidian vault
for any documents with `publish: true` in the frontmatter and copy them to the Quartz content directory.

This tool is currently under development and is not yet ready for use.

## Installation

1. Prerequisites:
   - .NET Core 9.0
2. Steps:
   ```sh
   # Clone the repository
   git clone https://github.com/chasebussey/QuartzPublisher.git

   # Navigate to the project directory
   cd QuartzPublisher

   # Install dependencies
   dotnet restore
   
## Usage
Building the project will create a `quartz-publish` executable in the `bin` directory. `quartz-publish` takes 3 arguments:
1. `-s` or `--source`: The path to the Obsidian vault directory.
2. `-d` or `--destination`: The path to the Quartz content directory.
3. `-v` or `--verbose`: Optional. Outputs additional information during the publishing process.