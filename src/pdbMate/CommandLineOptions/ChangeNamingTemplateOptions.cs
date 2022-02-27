using CommandLine;

namespace pdbMate.CommandLineOptions
{
    [Verb("changenaming", HelpText = "Rename filenames to new FilenameTemplate format - folders will not be changed.")]
    public class ChangeNamingTemplateOptions : BaseOptions
    {

    }
}
