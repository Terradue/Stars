using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Services.Router;

namespace Terradue.Stars.Services.Supplier
{
    public class GenericResource : IResource
    {
        private readonly Uri uri;

        public GenericResource(Uri uri)
        {
            this.uri = uri;
        }

        public ContentType ContentType => new ContentType("application/octet-stream");

        public ResourceType ResourceType => ResourceType.Unknown;

        public ulong ContentLength => 0;

        public ContentDisposition ContentDisposition => null;

        public Uri Uri => uri;
    }
}