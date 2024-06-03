// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StacNode.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Stac;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Services.Model.Stac
{
    public abstract class StacNode : IResource, IStreamResource, ILocatable
    {
        protected IStacObject stacObject;
        private Uri uri;

        protected StacNode(IStacObject stacObject, Uri uri = null)
        {
            if (stacObject == null)
                throw new ArgumentNullException("stacObject");
            if (!uri.IsAbsoluteUri)
                throw new ArgumentException("STAC uri must be an absolute uri");
            this.stacObject = stacObject;
            this.uri = uri ?? new Uri(Id + ".json", UriKind.Relative);
        }

        public string Title => stacObject.Title;

        public ContentType ContentType => stacObject.MediaType;

        public Uri Uri { get => uri; set => uri = value; }

        public abstract ResourceType ResourceType { get; }

        public string Id => stacObject.Id.CleanIdentifier();

        public virtual ulong ContentLength
        {
            get
            {
                MemoryStream ms = (MemoryStream)GetStreamAsync(CancellationToken.None).GetAwaiter().GetResult();
                return Convert.ToUInt64(ms.Length);
            }
        }

        public bool IsCatalog => (stacObject is IStacCatalog);

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = FileName };

        private string filename = null;
        public string FileName
        {
            get
            {
                if (!string.IsNullOrEmpty(filename))
                    return filename;
                if (ResourceType == ResourceType.Catalog)
                    return "catalog.json";
                return Id + ".json";
            }
            set
            {
                filename = value;
            }
        }

        public static StacNode Create(IStacObject stacObject, Uri uri)
        {
            if (stacObject is StacCollection)
                return new StacCollectionNode(stacObject as StacCollection, uri);

            if (stacObject is IStacCatalog)
                return new StacCatalogNode(stacObject as IStacCatalog, uri);

            if (stacObject is StacItem)
                return new StacItemNode(stacObject as StacItem, uri);

            return null;
        }

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return stacObject.Links.Select(l => new StacResourceLink(l)).ToList();
        }


        public IStacObject StacObject => stacObject;

        public bool CanBeRanged => false;

        public abstract IReadOnlyList<IResource> GetRoutes(IRouter router);

        public async Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            MemoryStream ms = new MemoryStream();
            return await Task<Stream>.Run(() =>
            {
                var sw = new StreamWriter(ms);
                sw.Write(StacConvert.Serialize(stacObject));
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return ms as Stream;
            }, ct);

        }

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }

    }
}
