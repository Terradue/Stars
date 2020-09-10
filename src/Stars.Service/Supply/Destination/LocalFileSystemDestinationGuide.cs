using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stars.Interface.Supply.Destination;

namespace Stars.Service.Supply.Destination
{
    public class LocalFileSystemDestinationGuide : IDestinationGuide
    {
        private readonly ILogger logger;

        public LocalFileSystemDestinationGuide(ILogger logger)
        {
            this.logger = logger;
        }

        public string Id => "LocalFS";

        public bool CanGuide(string destination)
        {
            try {
                FileAttributes fa = File.GetAttributes(destination.Replace("file://", "").TrimEnd('/'));
                return (fa & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch (Exception e) {
                logger.LogWarning(e.Message);
                return false;
            }
        }

        public Task<IDestination> Guide(string destination)
        {
            return Task.FromResult((IDestination)LocalDirectoryDestination.Create(destination));
        }
    }
}