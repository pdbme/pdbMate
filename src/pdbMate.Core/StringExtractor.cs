using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace pdbMate.Core
{
    public static class StringExtractor 
    {
        public static DateTime? ExtractDate(string title)
        {
            var match = Regex.Match(title, @"^([a-z0-9-_]+)\.([0-9]{2})\.([0-9]{2})\.([0-9]{2})",
                RegexOptions.IgnoreCase);

            if (match.Success)
            {
                var dateString = $"{match.Groups[2].Value}-{match.Groups[3].Value}-{match.Groups[4].Value}";

                if (DateTime.TryParseExact(dateString, "yy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    return dt;
                }

                if (DateTime.TryParseExact(dateString, "dd-MM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2))
                {
                    return dt2;
                }
            }
            else
            {
                var matchFourDigit = Regex.Match(title, @"^([a-z0-9-_]+)\.([0-9]{4})\.([0-9]{2})\.([0-9]{2})",
                    RegexOptions.IgnoreCase);

                if (matchFourDigit.Success)
                {
                    var dateString = $"{matchFourDigit.Groups[2].Value}-{matchFourDigit.Groups[3].Value}-{matchFourDigit.Groups[4].Value}";

                    if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2))
                    {
                        return dt2;
                    }
                }
            }

            return null;
        }

        public static string ReplaceSpacesByDots(string s)
        {
            s = s.Replace(" ", ".");
            return s;
        }

        public static string ExtractTeam(string title)
        {
            title = title.Replace("-Pornfuscated", "");

            Match matchTeamname = Regex.Match(title, @"(2160p|1080p|720p|480)\.(MP4|WMV|MKV)-([a-z0-9]+)$", RegexOptions.IgnoreCase);
            if (matchTeamname.Success)
            {
                return matchTeamname.Groups[3].Value;
            }

            return "";
        }


        public static string ExtractEpisode(string title)
        {
            string episode = "";
            title = ReplaceSpacesByDots(title);
            Match match = Regex.Match(title, @"\.E([0-9]{2,4})\.", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                episode = match.Groups[1].Value;
            }

            return episode;
        }

        public static string ExtractQuality(string s)
        {
            s = s.Replace(" ", ".");
            s = s.Replace("_", ".");
            s = s.Replace(",", ".");
            s = s.Replace("-", ".");

            Match matchTeamname = Regex.Match(s, @"\.(2160p|1080p|720p|480p)\.", RegexOptions.IgnoreCase);
            if (matchTeamname.Success)
            {
                return matchTeamname.Groups[1].Value;
            }

            return "";
        }
    }
}
