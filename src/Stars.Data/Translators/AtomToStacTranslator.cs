using System.Threading.Tasks;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.OpenSearch.Result;
using Terradue.Stars.Services.Model.Atom;
using Stac;
using Terradue.Stars.Services.Model.Stac;
using System.Collections.Generic;
using Terradue.Stars.Geometry.Atom;
using Terradue.Stars.Interface;
using System.Net;
using Terradue.Stars.Services.Plugins;

namespace Terradue.Stars.Data.Translators
{
    [PluginPriority(5)]
    public class AtomToStacTranslator : ITranslator
    {
        private AtomRouter atomRouter;
        
        public AtomToStacTranslator(ICredentials credentials)
        {
            this.atomRouter = new AtomRouter(credentials);
            Key = "AtomToStacTranslator";
        }

        public int Priority { get; set; }

        public string Key { get; set; }

        public string Label => "ATOM to STAC";

        public async Task<T> Translate<T>(IResource node) where T : IResource
        {
            AtomFeed atomFeed = await TryToFindAtomFeedInNode(node);
            if (atomFeed != null)
            {
                return (T)CreateStacCatalogNode(atomFeed);
            }
            AtomItem atomItem = await TryToFindAtomItemInNode(node);
            if (atomItem != null)
            {
                return (T)CreateStacItemNode(atomItem);
            }
            return default(T);
        }

        private IResource CreateStacCatalogNode(IOpenSearchResultCollection collection)
        {
            StacCatalog catalog = new StacCatalog(collection.CreateIdentifier(), collection.Title == null ? null : collection.Title.Text, CreateStacLinks(collection));
            return StacCatalogNode.CreateUnlocatedNode(catalog);
        }

        private IEnumerable<StacLink> CreateStacLinks(IOpenSearchResultCollection collection)
        {
            return null;
        }

        private IResource CreateStacItemNode(IOpenSearchResultItem osItem)
        {
            AtomItem item = Terradue.OpenSearch.Result.AtomItem.FromOpenSearchResultItem(osItem);
            StacItem stacItem = new StacItem(item.Identifier, item.FindGeometry(), item.GetCommonMetadata());
            return StacItemNode.CreateUnlocatedNode(stacItem);
        }

        private async Task<AtomItem> TryToFindAtomItemInNode(IResource node)
        {
            AtomItemNode atomItemRoutable = node as AtomItemNode;
            if (atomItemRoutable != null) return new AtomItem(atomItemRoutable.AtomItem);
            if (atomRouter.CanRoute(node)){
                atomItemRoutable = await atomRouter.Route(node) as AtomItemNode;
            }
            if (atomItemRoutable == null) return null;
            return new AtomItem(atomItemRoutable.AtomItem);
        }

        private async Task<AtomFeed> TryToFindAtomFeedInNode(IResource node)
        {
            AtomFeedCatalog atomFeedRoutable = node as AtomFeedCatalog;
            if (atomFeedRoutable != null) new AtomFeed(atomFeedRoutable.AtomFeed);
            if (atomRouter.CanRoute(node)){
                atomFeedRoutable = await atomRouter.Route(node) as AtomFeedCatalog;
            }
            if (atomFeedRoutable == null) return null;
            return new AtomFeed(atomFeedRoutable.AtomFeed);
        }

    }
}
