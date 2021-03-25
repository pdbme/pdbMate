# pdbMate
pdbMate supports you organizing your media library by automatic renaming and organizing by media sites.

# What does pdbMate do?

pdbMate is a tool with a Command Line Interface (CLI) that performs different tasks to help you handle files downloaded from usenet.

The tool is specialized for adult content and uses an API to gather information about sites, actors, video titles and release names.

We try to avoid any adult related terms in any code and documentation about pdbMate.

Why did we come up with pdbMate: There are no media manager that can sort adult content. pdbMate tries to fill this gap.
Functions

# Install

Download the latest release from releases for your operating system:

https://github.com/pdbme/pdbMate/releases

Install .NET framework 5.0 Runtime

https://dotnet.microsoft.com/download/dotnet/5.0/runtime

If build from source: Rename the appsettings.Template.json to appsettings.json 

Configure appsettings.json to your needs:
- define sourcefolders (folders that are recursively searched for videos to rename)
- define on targetfolder (your files will be moved to this folder)
- get an api key and fill out PdbApi -> ApiKey

# How to use

You can call the CLI with different functions, for example:

pdbMate rename --dryrun

The dryrun parameter means no files will be renamed. So you can check the results before renaming and moving your files.
