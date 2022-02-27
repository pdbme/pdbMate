# pdbMate

## Breaking changes
pdbMate has been updated to dotnet6

Install

Download the latest release from releases for your operating system:

https://github.com/pdbme/pdbMate/releases

Install .NET framework 6.0 Runtime

https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime

If build from source: Rename the appsettings.Template.json to appsettings.json

Configure appsettings.json to your needs:

    define sourcefolders (folders that are recursively searched for videos to rename)
    define on targetfolder (your files will be moved to this folder)
    get an api key and fill out PdbApi -> ApiKey


## What does pdbMate do?

pdbMate is a tool with a Command Line Interface (CLI) that performs different tasks to help you handle files downloaded from usenet. 

The tool is specialized for adult content and uses an API to gather information about sites, actors, video titles and release names.

We try to avoid any adult related terms in any code and documentation about pdbMate.

Why did we come up with pdbMate: There are no media manager that can sort adult content. pdbMate tries to fill this gap.

## Functions

You can call the CLI with different functions, for example:

# rename (testing with dryrun)
pdbMate rename --dryrun

# rename
pdbMate rename

# download (sabnzbd)
pdbMate download

# download (nzbget)
pdbMate download --client nzbget

# rename and download combined (sabnzbd)
pdbMate autopilot

# rename and download combined (nzbget)
pdbMate autopilot --client nzbget

The dryrun parameter means no files will be renamed. So you can check the results before renaming and moving your files.
The client parameter sets the usenet download client that should be used (sabnzbd or nzbget). Default value for this parameter is sabnzbd.

### Rename

Typical use-case: You are downloading adult content from usenet and want to sort your downloads by sites (one folder per adult website) and you want to sort out duplicate files. 
When downloading from usenet you typically have one release per folder. This is not optimal when using media managers like Plex. You want some kind of sorting in a reasonable folder structure.

Renaming and moving files makes sense when you want to watch videos on your local PC and even when adding content to your media managers. There are no media manager that can sort adult content.

What does the rename function do (Step-By-Step)?

- Gather a list of all files in the source folders you specified in your appsettings.json.
- Ignore folders and files that are not ready for renaming (unpack folders from sabnzbd)
- Querying the pdb API for sites and videos (results are cached during the execution).
- Sort out files that are to small or have the wrong file extension (mkv and mp4 only).
- Determine a matching video id from pdb API and getting the video quality (from folder or filename).
- Determine duplicate filenames
- Moving and renaming files depending on your settings from FilenameTemplate and FolderTemplate in appsettings.json.
- Deleting duplicates and unused files (small files etc.)

### Download

The download option enables you to automatically add nzb files to sabnzbd or nzbget for all newly released videos based on your favorite sites and actors (specified in porndb.me).

What does the download function do (Step-By-Step)?

 - pdbMate asks the porndb.me API with your API key what favorite sites and actors you have on record. 
 - It checks the directories you specified in the appsettings.json and builds a list of videos you already have on disk.
 - Then it asks the porndb.me API about new releases from your indexers (only those you have an api key on record with on porndb.me). 
 - Then it adds nzb files to either sabnzbd or nzbget for releases that you do not have on disk yet.

The major advantage with pdbMate download function in comparison to rss feeds is you do not get duplicates at all and you can specify prefered video qualities (video resolution).

Once you configured pdbMate correctly on your downloading machine, you can manage your favorite sites and stars on porndb.me and the rest is done automatically by pdbMate and your usenet downloader.

Configured correctly you now have an automated download functionality setup for porn from usenet. As far as we know this has never been accompished before. Custom rss feeds where the best option you had until now.

## Configuring pdbMate

In the same directory as pbbMate there is a appsettings.Template.json - please rename this file to appsettings.json.

Once renamed, you have to change some settings:

# porndb.me API
Most importantly you have to replace --MY-OWN-APIKEY-- with your personal api key at porndb.me.

# renaming config
You should specify one or more SourceFolders. Then you should specify the target folder where the renamed files will be moved to.
FilenameTemplate and FolderTemplte do not need to be changed, you can edit they to your personal preference if you like.

Examples for FilenameTemplate:

´´´
{Video.Site.Sitename} - {Video.Title} - {VideoQuality.SimplifiedName}.{PdbId}{FileExtension}
{Video.ReleasedateShort} - {Video.Title} - {VideoQuality.SimplifiedName}.{PdbId}{FileExtension}
{OriginalFilename}.{VideoQuality.SimplifiedName}.{PdbId}{FileExtension}
´´´

Available placeholders:
´´´
{Video.Site.Sitename}
{Video.Site.Id}
{Video.Id}
{Video.Title}
{Video.Releasedate}
{Video.ReleasedateShort}
{VideoQuality.SimplifiedName}
{VideoQuality.Name}
{FileExtension}
{PdbId}
{OriginalFilename}
´´´

# configure download client
You can set connection details to your favorite usenet download client. Both clients can be configured but you determine with your command line call arguments (e.g. --client nzbget) which client is used. The default client is sabnzbd.

# configure download settings
In UsenetDownload you can change:
- AllowedQualities: You decided which video quality the release must have to be downloaded.
- KeepOnlyHighestQuality: during renaming and only the highest quality of a release is kept, everything else is considered a duplicate and will be removed
- DownloadFavoriteActors: In autopilot mode all new releases from your favorite actors will be added to your download client
- DownloadFavoriteSites: In autopilot mode all new releases from your favorite sites will be added to your download client
