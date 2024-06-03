using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier.Destination;

namespace Terradue.Stars.Services.Supplier.Destination
{
    public class LocalFileSystemDestinationGuide : IDestinationGuide
    {
        private readonly ILogger logger;
        private readonly IFileSystem fileSystem;

        public LocalFileSystemDestinationGuide(ILogger<LocalFileSystemDestinationGuide> logger,
                                               IFileSystem fileSystem)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
        }

        public int Priority { get; set; }

        public string Key { get => Id; set { } }

        public string Id => "LocalFS";

        public bool CanGuide(string directory, IResource route)
        {
            try
            {
                return directory.StartsWith("/") || directory.StartsWith("file:/");
            }
            catch (Exception e)
            {
                logger.LogWarning(e.Message);
                return false;
            }
        }

        public Task<IDestination> Guide(string directory, IResource route)
        {
            var dir = fileSystem.DirectoryInfo.FromDirectoryName(directory.Replace("file:", "").TrimEnd('/'));
            if (!dir.Exists && !dir.Parent.Exists)
                throw new InvalidOperationException(string.Format("{0} directory does not exist", dir.Parent.FullName));
            return Task.FromResult<IDestination>(LocalFileDestination.Create(dir, route));
        }
    }
}
