using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier.Destination;
using Terradue.Stars.Services.Supplier.Destination;

namespace Terradue.Stars.Services.Router
{
    public class LocalDirectoryResource : IAssetsContainer
    {
        private readonly IFileSystem fileSystem;
        private IDirectoryInfo dirInfo;

        public LocalDirectoryResource(IFileSystem fileSystem, string dirPath)
        {
            this.dirInfo = fileSystem.DirectoryInfo.FromDirectoryName(dirPath);
            this.fileSystem = fileSystem;
        }

        public IReadOnlyDictionary<string, IAsset> Assets => dirInfo.EnumerateFiles().ToDictionary(f => f.Name, f => new LocalFileResource(fileSystem, f.FullName));
        public Uri Uri => throw new NotImplementedException();
    }
}