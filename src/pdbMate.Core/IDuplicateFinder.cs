﻿using System.Collections.Generic;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public interface IDuplicateFinder
    {
        List<RenamerResult> Process(List<RenamerResult> renameResults, bool includeQuality);
    }
}