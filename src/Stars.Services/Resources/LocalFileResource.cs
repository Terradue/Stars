// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: LocalFileResource.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Router
{
    public class LocalFileResource : IStreamResource, IDeletableResource
    {
        private readonly ResourceType resourceType;
        private IFileInfo fileInfo;
        private List<string> roles;

        public LocalFileResource(IFileSystem fileSystem, string filePath, ResourceType ResourceType, List<string> roles = null)
        {
            fileInfo = fileSystem.FileInfo.FromFileName(filePath);
            resourceType = ResourceType;
            this.roles = new List<string>();
            if (roles != null)
                this.roles.AddRange(roles);
        }

        public Uri Uri => new Uri("file://" + fileInfo.FullName);

        public ContentType ContentType => new ContentType(MimeTypes.GetMimeType(fileInfo.Name));

        public ResourceType ResourceType => resourceType;

        public ulong ContentLength => Convert.ToUInt64(fileInfo.Length);

        public IFileInfo File => fileInfo;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = fileInfo.Name };

        public bool CanBeRanged => false;

        public string Title => fileInfo.FullName;

        public IReadOnlyList<string> Roles => roles;

        public IReadOnlyDictionary<string, object> Properties => new Dictionary<string, object>();

        public IEnumerable<IAsset> Alternates => Enumerable.Empty<IAsset>();

        public Task DeleteAsync(CancellationToken ct)
        {
            fileInfo.Delete();
            return Task.CompletedTask;
        }

        public Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            return Task.FromResult(fileInfo.OpenRead());
        }

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return fileInfo.FullName;
        }
    }
}
