using System;
using System.IO;
using System.Text.RegularExpressions;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Service
{
    public static class StarsExtensions
    {
        public static string CleanIdentifier(this string strIn)
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

        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        /// <summary>Indicates whether the specified array is null or has a length of zero.</summary>
        /// <param name="array">The array to test.</param>
        /// <returns>true if the array parameter is null or has a length of zero; otherwise, false.</returns>
        public static bool IsNullOrEmpty(this Array array)
        {
            return (array == null || array.Length == 0);
        }

        public static string ReadAsString(this IStreamable streamable)
        {
            StreamReader sr = new StreamReader(streamable.GetStreamAsync().Result);
            return sr.ReadToEnd();
        }
    }
}