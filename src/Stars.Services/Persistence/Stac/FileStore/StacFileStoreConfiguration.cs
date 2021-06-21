using System;
using System.Runtime.Serialization;

namespace Terradue.Stars.Services.Persistence.Stac.FileStore
{
    public class StacFileStoreConfiguration
    {
        public FileStoreNode RootCatalog { get; set; }
        public bool AbsoluteUris { get; set; }
        public TimeSpan LockTimeout { get; set; }
        public bool RelativeReference { get; set; }
    }

    public class FileStoreNode
    {
        public string Url { get; set; }
        public string Identifier { get; set; }
        public string Description { get; set; }
        public string FrontEndUrl { get; set; }
    }
}