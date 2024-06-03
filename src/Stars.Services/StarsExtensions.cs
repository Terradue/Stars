using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Services.Credentials;

namespace Terradue.Stars.Services
{
    public static class StarsExtensions
    {
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, value);
                return true;
            }
            return false;
        }

        public static string CleanIdentifier(this string strIn)
        {
            if (string.IsNullOrEmpty(strIn))
                return null;
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\w\.-]", "_",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return string.Empty;
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

        public async static Task<string> ReadAsStringAsync(this IStreamResource streamable, CancellationToken ct)
        {
            StreamReader sr = new StreamReader(await streamable.GetStreamAsync(ct));
            return await sr.ReadToEndAsync();
        }

        public static CredentialsConfigurationSection ToCredentialsConfigurationSection(this ICredentials cred, Uri uri, string authType)
        {
            CredentialsConfigurationSection credentialsConfigurationSection = new CredentialsConfigurationSection();
            if (cred is NetworkCredential)
            {
                credentialsConfigurationSection.AuthType = authType;
                credentialsConfigurationSection.UriPrefix = uri.ToString();
                credentialsConfigurationSection.Username = (cred as NetworkCredential).UserName;
                credentialsConfigurationSection.Password = (cred as NetworkCredential).Password;
            }
            return credentialsConfigurationSection;
        }
    }
}
