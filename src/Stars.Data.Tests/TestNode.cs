using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GeoJSON.Net.Geometry;
using Itenso.TimePeriod;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Supplier;

namespace Terradue.Data.Tests
{
    internal class TestItem : IItem
    {
        private DirectoryInfo directory;

        public TestItem(string folderPath)
        {
            this.directory = new DirectoryInfo(folderPath);
        }

        public string Title => directory.FullName;

        public string Id => directory.Name;

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
                    assets.Add(Path.GetRelativePath(directory.FullName, file.FullName), asset);
                }
                return assets;
            }
        }

        public ITimePeriod DateTime => new TimeInterval(directory.CreationTime);

        public IReadOnlyList<IResourceLink> GetLinks()
        {
            return new List<IResourceLink>();
        }

        public Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }
    }
}