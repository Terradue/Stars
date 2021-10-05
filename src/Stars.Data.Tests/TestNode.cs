using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;

namespace Terradue.Data.Test
{
    internal class TestItem : IItem
    {
        private DirectoryInfo directory;

        public TestItem(string folderPath)
        {
            this.directory = new DirectoryInfo(folderPath);
        }

        public string Label => directory.FullName;

        public string Id => directory.FullName;

        public bool IsCatalog => false;

        public Uri Uri => new Uri(Path.Combine(directory.FullName, "../.."));

        public ContentType ContentType => new ContentType("application/x-directory");

        public ResourceType ResourceType => ResourceType.Item;

        public ulong ContentLength => 0;

        public DirectoryInfo Directory { get => directory; }

        public IGeometryObject Geometry => null;

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = Path.GetFileNameWithoutExtension(directory.FullName) };

        public IDictionary<string, object> Properties => new Dictionary<string, object>();

        public bool CanBeRanged => false;

        public IReadOnlyDictionary<string, IAsset> Assets
        {
            get
            {
                Dictionary<string, IAsset> assets = new Dictionary<string, IAsset>();
                foreach (var file in directory.GetFiles("*", new EnumerationOptions() { RecurseSubdirectories = true }))
                {
                    var asset = new TestFileAsset(file, new Uri(directory.FullName), Uri);
                    assets.Add(file.FullName, asset);
                }
                return assets;
            }
        }

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return new List<IResourceLink>();
        }

        public Task<Stream> GetStreamAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetStreamAsync(long start, long end = -1)
        {
            throw new NotImplementedException();
        }
    }
}