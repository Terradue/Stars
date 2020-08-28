using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Stars.Interface.Supply.Destination;

namespace Stars.Supply.Destination
{
    public class LocalFileSystemDestinationGuide : IDestinationGuide
    {
        private readonly IReporter reporter;

        public LocalFileSystemDestinationGuide(IReporter reporter)
        {
            this.reporter = reporter;
        }

        public string Id => "LocalFS";

        public bool CanGuide(string destination)
        {
            try {
                FileAttributes fa = File.GetAttributes(destination.Replace("file://", "").TrimEnd('/'));
                return (fa & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch (Exception e) {
                reporter.Warn(e.Message);
                return false;
            }
        }

        public Task<IDestination> Guide(string destination)
        {
            return Task.FromResult((IDestination)LocalDirectoryDestination.Create(destination));
        }
    }
}