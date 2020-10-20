using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Terradue.Stars.Console
{
    internal class ConsoleUserSettings
    {
        private readonly string userSettingsFilePath = Path.Join(System.Environment.GetEnvironmentVariable("HOME"), ".config", "Stars" , "usersettings.json");
        private readonly ILogger logger;
        private readonly IConfigurationRoot configuration;

        public ConsoleUserSettings(ILogger logger, IConfigurationRoot configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
            try
            {
                if (!File.Exists(userSettingsFilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(userSettingsFilePath));
                    File.WriteAllText(userSettingsFilePath, "{}");
                }

            }
            catch (Exception ex)
            {
                logger.LogError("Error creating user app settings : {0}", ex.Message);
            }
        }

        public void AddOrUpdateSetting<T>(string sectionPathKey, T value)
        {
            if (string.IsNullOrEmpty(sectionPathKey) || value == null)
                return;
            try
            {
                string json = File.ReadAllText(userSettingsFilePath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                jsonObj ??= new JObject();
                SetValueRecursively(sectionPathKey, jsonObj, value);

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(userSettingsFilePath, output);

            }
            catch (Exception ex)
            {
                logger.LogError("Error writing user app settings : {0}", ex.Message);
                logger.LogDebug(ex.StackTrace);
            }
            configuration.Reload();
        }

        private void SetValueRecursively<T>(string sectionPathKey, dynamic jsonObj, T value)
        {
            // split the string at the first ':' character
            var remainingSections = sectionPathKey.Split(":", 2);

            var currentSection = remainingSections[0];
            if (remainingSections.Length > 1)
            {
                // continue with the procress, moving down the tree
                var nextSection = remainingSections[1];
                jsonObj[currentSection] ??= new JObject();
                SetValueRecursively(nextSection, jsonObj[currentSection], value);
            }
            else
            {
                // we've got to the end of the tree, set the value
                jsonObj[currentSection] = JToken.FromObject(value);
            }
        }
    }
}