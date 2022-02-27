using CommandLine;

namespace pdbMate.CommandLineOptions
{
    public class BaseOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('d', "dryrun", Required = false, HelpText = "Run without performing changes to the filesystem.")]
        public bool DryRun { get; set; }

        [Option('c', "client", Required = false, HelpText = "Specify download client (e.g. nzbget or sabnzbd).", Default = "sabnzbd")]
        public string Client { get; set; }
    }
}
