# pdbMate

## What does pdbMate do?

pdbMate is a tool with a Command Line Interface (CLI) that performs different tasks to help you handle files downloaded from usenet. 

The tool is specialized for adult content and uses an API to gather information about sites, actors, video titles and release names.

We try to avoid any adult related terms in any code and documentation about pdbMate.

Why did we come up with pdbMate: There are no media manager that can sort adult content. pdbMate tries to fill this gap.

## Functions

You can call the CLI with different functions, for example:

pdbMate rename --dryrun

The dryrun parameter means no files will be renamed. So you can check the results before renaming and moving your files.

### Rename

Typical use-case: You are downloading adult content from usenet and want to sort your downloads by sites (one folder per adult website) and you want to sort out duplicate files. 
When downloading from usenet you typically have one release per folder. This is not optimal when using media managers like Plex. You want some kind of sorting in a reasonable folder structure.

Renaming and moving files makes sense when you want to watch videos on your local PC and even when adding content to your media managers. There are no media manager that can sort adult content.

What does the rename function do?

- Gather a list of all files in the source folders you specified in your appsettings.json.
- Ignore folders and files that are not ready for renaming (unpack folders from sabnzbd)
- Querying the pdb API for sites and videos (results are cached during the execution).
- Sort out files that are to small or have the wrong file extension (mkv and mp4 only).
- Determine a matching video id from pdb API and getting the video quality (from folder or filename).
- Determine duplicate filenames
- Moving and renaming files depending on your settings from FilenameTemplate and FolderTemplate in appsettings.json.
- Deleting duplicates and unused files (small files etc.)

### Best use-case

- 