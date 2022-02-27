using System;
using System.Collections.Generic;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public interface IChangeNamingTemplateService
    {
        void RenameToNewTemplate(bool dryRun);
    }
}