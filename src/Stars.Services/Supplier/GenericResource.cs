using System;
using System.Net.Mime;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services
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
