using System;
using System.Net;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Service;

namespace Terradue.Stars.Operations
{
    internal class ConsoleCredentialsManager : ConfigurationCredentialsManager
    {
        private readonly IConsole console;
        private readonly ConsoleUserSettings consoleUserSettings;

        public ConsoleCredentialsManager(IOptions<CredentialsOptions> options, IConsole console, ConsoleUserSettings consoleUserSettings, ILogger logger) : base(options, logger)
        {
            this.console = console;
            this.consoleUserSettings = consoleUserSettings;
        }

        public override NetworkCredential GetCredential(Uri uri, string authType)
        {
            NetworkCredential cred = base.GetCredential(uri, authType);
            if (cred == null)
            {
                if (!console.IsInputRedirected)
                {
                    console.WriteLine("No credentials found for {0}. Please provide one.", uri);
                    cred = PromptCredentials(uri, authType);
                }
                if (cred != null)
                {
                    PromptSaveCredentials(cred, uri);
                }
            }
            return cred;
        }

        private void PromptSaveCredentials(ICredentials cred, Uri uri)
        {
            string answer = "p";
            int i = 3;
            while ((string.IsNullOrEmpty(answer) || !"yn".Contains(answer[0])) && i > 0)
            {
                console.Write("Save credentials in user settings? [y/N]:");
                answer = console.In.ReadLine().ToLower();
                i--;
            }
            if (answer == "y")
                SaveCredentials(cred, uri);
        }

        private void SaveCredentials(ICredentials cred, Uri uri)
        {
            CredentialsConfigurationSection credConfigSection = cred.ToCredentialsConfigurationSection(uri);
            consoleUserSettings.AddOrUpdateSetting<CredentialsConfigurationSection>("Credentials:" + Guid.NewGuid().ToString(), credConfigSection);
        }

        private NetworkCredential PromptCredentials(Uri uri, string authType)
        {
            if (console.IsInputRedirected) return null;

            string username = null;
            int rtry = 3;
            while (string.IsNullOrEmpty(username) && rtry > 0)
            {
                console.Write("username: ");
                username = console.In.ReadLine();
                rtry--;
            }
            if (string.IsNullOrEmpty(username))
            {
                console.WriteLine("No input. Skipping");
                return null;
            }
            console.Write("password: ");
            var pass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);
            console.WriteLine();

            return new NetworkCredential(username, pass);
        }
    }
}