// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: LocalDirectoryResource.cs

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Router
{
    public class LocalDirectoryResource : IAssetsContainer
    {
        private readonly IFileSystem fileSystem;
        private readonly IDirectoryInfo dirInfo;

        public LocalDirectoryResource(IFileSystem fileSystem, string dirPath)
        {
            dirInfo = fileSystem.DirectoryInfo.FromDirectoryName(dirPath);
            this.fileSystem = fileSystem;
        }

        public IReadOnlyDictionary<string, IAsset> Assets => dirInfo.EnumerateFiles().ToDictionary(f => f.Name, f => (IAsset)new LocalFileResource(fileSystem, f.FullName, ResourceType.Unknown));
        public Uri Uri => throw new NotImplementedException();
    }
}
