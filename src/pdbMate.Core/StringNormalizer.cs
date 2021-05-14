using System.Text.RegularExpressions;

namespace pdbMate.Core
{
    public static class StringNormalizer
    {
        public static string Normalize(string s)
        {
            s = Regex.Replace(s, @"[^a-zA-Z]+", " ");
            s = s.ToLowerInvariant();
            s = s.Replace("     ", " ");
            s = s.Replace("    ", " ");
            s = s.Replace("   ", " ");
            s = s.Replace("  ", " ");
            s = s.Replace("  ", " ");
            s = s.Trim();

            return s;
        }
    }
}
