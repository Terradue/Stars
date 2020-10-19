using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier.Destination
{
    public class LocalFileSystemDestinationGuide : IDestinationGuide
    {
        private readonly ILogger logger;

        public LocalFileSystemDestinationGuide(ILogger logger)
        {
            this.logger = logger;
        }

        public int Priority { get; set; }

        public string Key { get => Id; set { } }

        public string Id => "LocalFS";

        public bool CanGuide(string destination)
        {
            try
            {
                FileAttributes fa = File.GetAttributes(destination.Replace("file:", "").TrimEnd('/'));
                return (fa & FileAttributes.Directory) == FileAttributes.Directory;
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

        public Task<IDestination> Guide(string destination)
        {
            FileAttributes fa = File.GetAttributes(destination.Replace("file:", "").TrimEnd('/'));
            if ((fa & FileAttributes.Directory) != FileAttributes.Directory)
                throw new InvalidOperationException(string.Format("{0} is not a directory", destination));
            return Task.FromResult((IDestination)LocalDirectoryDestination.Create(destination.Replace("file:", "").TrimEnd('/')));
        }
    }
}