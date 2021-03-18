using System.Collections.Generic;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public interface IFileOperatingService
    {
        void RemoveUnwantedFiles(List<SourceFile> files, bool dryRun);
        void MoveFilesBasedOnRenameResults(List<RenamerResult> renamerResults, string targetFolder, bool dryRun);
        void DeleteDirectories(List<string> directories, bool dryRun);

        void WriteJsonFromObject<T>(T objectClass, string filename);
    }
}