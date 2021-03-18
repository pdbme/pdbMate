using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using pdbMate.Core.Data;

namespace pdbMate.Core
{
    public class DuplicateFinder : IDuplicateFinder
    {
        private readonly ILogger<IDuplicateFinder> logger;

        public DuplicateFinder(ILogger<IDuplicateFinder> logger)
        {
            this.logger = logger;
        }

        public List<RenamerResult> Process(List<RenamerResult> renameResults, bool includeQuality)
        {
            List<string> alreadyExistingKeys = new List<string>();
            foreach (var renameResult in renameResults)
            {
                if (renameResult.Video == null)
                {
                    continue;
                }

                string key = renameResult.Video.Id.ToString();
                if (includeQuality)
                {
                    key = renameResult.Video.Id + "-" + renameResult.VideoQualityId;
                }

                if (alreadyExistingKeys.Contains(key))
                {
                    renameResult.Source.IsDuplicate = true;
                }
                else
                {
                    alreadyExistingKeys.Add(key);
                }
            }

            return renameResults;
        }
    }
}