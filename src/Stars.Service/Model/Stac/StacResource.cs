using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stac;
using Stac.Catalog;
using Stac.Item;
using Stars.Interface.Model;
using Stars.Interface.Router;
using Stars.Interface.Supply;
using Stars.Service.Router;

namespace Stars.Service.Model.Stac
{
    internal abstract class StacResource : IRoutable, IStacNode, INode, IStreamable
    {
        protected IStacObject stacObject;
        protected ContentType contentType;

        public StacResource(IStacObject stacObject)
        {
            this.stacObject = stacObject;
            this.contentType = new ContentType("application/json");
        }

        public string Label => stacObject.Id;

        public ContentType ContentType => contentType;

        public Uri Uri => stacObject.Uri;

        public abstract ResourceType ResourceType { get; }

        public string Id => stacObject.Id.CleanIdentifier();

        public virtual ulong ContentLength => Convert.ToUInt64(Encoding.Default.GetBytes(JsonConvert.SerializeObject(stacObject)).Length);

        public bool IsCatalog => (stacObject is IStacCatalog);

        public ContentDisposition ContentDisposition => new ContentDisposition() { FileName = FileName };

        public string FileName
        {
            get
            {
                return stacObject.Id + ".json";

            }
        }

        public abstract string ReadAsString();

        internal static StacResource Create(IStacObject stacObject)
        {
            if (stacObject is IStacCatalog)
                return new StacCatalogResource(stacObject as IStacCatalog);

            if (stacObject is IStacItem)
                return new StacItemResource(stacObject as IStacItem);

            return null;
        }


        public Task<INode> GoToNode()
        {
            return Task.FromResult((INode)this);
        }

        public Task<IStacObject> GetStacObject()
        {
            return Task.FromResult(stacObject);
        }

        public abstract IList<IRoute> GetRoutes();

        public Task<Stream> GetStreamAsync()
        {
            throw new NotImplementedException();
        }
    }
}