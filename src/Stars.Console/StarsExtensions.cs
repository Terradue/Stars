using System;
using System.Text.RegularExpressions;

namespace Stars
{
    internal static class StarsExtensions
    {
        internal static string CleanIdentifier(this string strIn)
        {
            if (string.IsNullOrEmpty(strIn))
                return null;
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@-]", "_",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        internal static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }
    }
}