// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: ConsoleCredentialsManager.cs

using System;
using System.Net;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Terradue.Stars.Services;
using Terradue.Stars.Services.Credentials;

namespace Terradue.Stars.Console.Operations
{
    internal class ConsoleCredentialsManager : ConfigurationCredentialsManager
    {
        private readonly IConsole console;
        private readonly ConsoleUserSettings consoleUserSettings;

        public ConsoleCredentialsManager(IOptions<CredentialsOptions> options, IConsole console, ConsoleUserSettings consoleUserSettings, ILogger<ConsoleCredentialsManager> logger) : base(options, logger)
        {
            this.console = console;
            this.consoleUserSettings = consoleUserSettings;
        }

        public override NetworkCredential GetCredential(Uri uri, string authType)
        {
            var authority = uri.GetLeftPart(UriPartial.Authority);
            if (string.IsNullOrEmpty(authority)) { authority = "/"; }
            NetworkCredential cred = null;
            Uri uriCut = new Uri(authority);
            try
            {
                UriBuilder uriCutBuilder = new UriBuilder(uriCut);
                uriCutBuilder.UserName = null;
                uriCutBuilder.Password = null;
                cred = base.GetCredential(uriCutBuilder.Uri, authType);
            }
            catch { }
            if (cred == null && authority.Length > 2 && uri.UserInfo != "preauth")
            {
                if (!console.IsInputRedirected)
                {
                    console.WriteLine("No credentials found for {0}. Please provide one.", uriCut);
                    cred = PromptCredentials(uriCut, authType);
                }
                if (cred != null)
                {
                    CacheCredential(uriCut, authType, cred);
                    PromptSaveCredentials(cred, uriCut, authType);
                }
            }
            return cred;
        }

        private void PromptSaveCredentials(ICredentials cred, Uri uri, string authType)
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
                SaveCredentials(cred, uri, authType);
        }

        private void SaveCredentials(ICredentials cred, Uri uri, string authType)
        {
            CredentialsConfigurationSection credConfigSection = cred.ToCredentialsConfigurationSection(uri, authType);
            consoleUserSettings.AddOrUpdateSetting("Credentials:" + Guid.NewGuid().ToString(), credConfigSection);
        }

        private NetworkCredential PromptCredentials(Uri uri, string authType)
        {
            if (console.IsInputRedirected) return null;

            string usernameLabel = "username";
            string passwordLabel = "password";

            if (authType.Equals("s3", StringComparison.InvariantCultureIgnoreCase))
            {
                usernameLabel = "S3 Key Id";
                passwordLabel = "S3 Secret";
            }

            string username = null;
            int rtry = 3;
            while (string.IsNullOrEmpty(username) && rtry > 0)
            {
                console.Write(usernameLabel + ": ");
                username = console.In.ReadLine();
                rtry--;
            }
            if (string.IsNullOrEmpty(username))
            {
                console.WriteLine("No input. Skipping");
                return null;
            }
            console.Write(passwordLabel + ": ");
            var pass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = System.Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    System.Console.Write("\b \b");
                    pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    System.Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);
            console.WriteLine();

            return new NetworkCredential(username, pass);
        }
    }
}
