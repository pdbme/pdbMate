using System.Collections.Generic;

namespace pdbMate.Core
{
    public class RenameServiceOptions
    {
        public string FilenameTemplate { get; set; }
        public string FolderTemplate { get; set; }
        public List<string> SourceFolders { get; set; }
        public string TargetFolder { get; set; }
    }
}
