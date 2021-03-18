using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public class FileOperatingService : IFileOperatingService
    {
        private readonly ILogger<IFileOperatingService> logger;


        public FileOperatingService(ILogger<IFileOperatingService> logger)
        {
            this.logger = logger;
        }

        public void RemoveUnwantedFiles(List<SourceFile> files, bool dryRun)
        {
            logger.LogInformation("==============");
            logger.LogInformation(" Remove files ");
            logger.LogInformation("==============");
            foreach (var file in files)
            {
                if (file.IsNoVideoExtension || file.IsToSmall || file.IsDuplicate)
                {
                    if (!dryRun && File.Exists(file.Path))
                    {
                        File.Delete(file.Path);
                    }
                    logger.LogInformation("Deleting " + file.Path);
                }
            }
        }

        public void MoveFilesBasedOnRenameResults(List<RenamerResult> renamerResults, string targetFolder, bool dryRun)
        {
            logger.LogInformation("==============");
            logger.LogInformation(" Move files to");
            logger.LogInformation("==============");

            foreach (var renamerResult in renamerResults)
            {
                if (renamerResult.Source.IsDuplicate)
                {
                    if (!dryRun && File.Exists(renamerResult.Source.Path))
                    {
                        File.Delete(renamerResult.Source.Path);
                    }
                    logger.LogInformation("Deleting duplicate file " + renamerResult.Source.Path);
                    continue;
                }

                if (renamerResult.Source.IsNoVideoExtension || renamerResult.Source.IsToSmall)
                {
                    if (!dryRun && File.Exists(renamerResult.Source.Path))
                    {
                        File.Delete(renamerResult.Source.Path);
                    }
                    logger.LogInformation("Deleting non-video or small file " + renamerResult.Source.Path);
                    continue;
                }

                if (renamerResult.Video == null)
                {
                    logger.LogWarning($"No video found for source filename: {renamerResult.Source.Filename}");
                    continue;
                }

                string sourceFile = renamerResult.Source.Path;

                string targetFolderWithSubfolder = string.IsNullOrEmpty(renamerResult.Foldername)
                    ? ""
                    : Path.Combine(targetFolder, renamerResult.Foldername);
                string targetFile = (string.IsNullOrEmpty(renamerResult.Foldername)) ?
                    Path.Combine(targetFolder, renamerResult.Filename) :
                    Path.Combine(targetFolder, renamerResult.Foldername, renamerResult.Filename);

                if (!dryRun)
                {
                    if (!File.Exists(sourceFile))
                    {
                        logger.LogError($"Source file {sourceFile} cound not be found.");
                        continue;
                    }

                    if (File.Exists(targetFile))
                    {
                        logger.LogError($"Target file {targetFile} already exists.");
                        continue;
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(targetFolderWithSubfolder) && !Directory.Exists(targetFolderWithSubfolder))
                        {
                            Directory.CreateDirectory(targetFolderWithSubfolder);
                        }

                        File.Move(sourceFile, targetFile);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Moving {sourceFile} to {targetFile} failed with Exception: {ex.Message}.");
                        break;
                    }

                }
                logger.LogInformation(targetFile);
            }
        }

        public void DeleteDirectories(List<string> directories, bool dryRun)
        {
            if (dryRun) return;
            foreach (var directoryPath in directories)
            {
                Directory.Delete(directoryPath);
            }
        }

        public void WriteJsonFromObject<T>(T objectClass, string filename)
        {
            var jsonString = JsonSerializer.Serialize<T>(objectClass);
            File.WriteAllText(filename, jsonString);
        }
    }
}
