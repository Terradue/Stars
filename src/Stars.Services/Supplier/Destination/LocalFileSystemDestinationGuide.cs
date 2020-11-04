using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier.Destination
{
    public class LocalFileSystemDestinationGuide : IDestinationGuide
    {
        private readonly ILogger logger;

        public LocalFileSystemDestinationGuide(ILogger<LocalFileSystemDestinationGuide> logger)
        {
            this.logger = logger;
        }

        public int Priority { get; set; }

        public string Key { get => Id; set { } }

        public string Id => "LocalFS";

        public bool CanGuide(string directory, IRoute route)
        {
            try
            {
                var dir = new DirectoryInfo(directory.Replace("file:", "").TrimEnd('/'));
                return true;
            }
            catch (Exception e)
            {
                logger.LogWarning(e.Message);
                return false;
            }
        }

        public void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {

        }

        public Task<IDestination> Guide(string directory, IRoute route)
        {
            var dir = new DirectoryInfo(directory.Replace("file:", "").TrimEnd('/'));
            if (!dir.Exists && !dir.Parent.Exists )
                throw new InvalidOperationException(string.Format("{0} directory does not exist", directory));
            return Task.FromResult<IDestination>(LocalFileDestination.Create(directory.Replace("file:", "").TrimEnd('/'), route));
        }
    }
}