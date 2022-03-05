using System.Collections.Generic;
using pdbMate.Core.Data;

namespace pdbMate.Core.Interfaces
{
    public interface IRenameService
    {
        List<SourceFile> GetFilesToProcess();
        List<RenamerResult> ProcessBatch(List<SourceFile> sourceFiles);
        List<RenamerResult> ProcessFiles(List<SourceFile> sourceFiles);
        List<string> GetEmptyDirectories();
        List<KnownVideoQualityResult> GetKnownVideoQualityResults();

        List<RenamerResult> CheckForDuplicates(List<RenamerResult> renamerResults,
            List<KnownVideoQualityResult> knownVideoQualityResults);

        List<RenamerResult> CheckForDuplicatesInResults(List<RenamerResult> renamerResults);
        string GetTargetPath();
        string ReplaceTemplatePlaceholders(string template, RenamerResult v);
    }
}