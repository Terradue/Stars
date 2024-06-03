using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stac;
using Terradue.Stars.Data.Model.Atom;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Services.Model.Atom;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.ThirdParty.Egms;
using Terradue.Stars.Services.ThirdParty.Titiler;

namespace Terradue.Stars.Data.Translators
{
    public class StacCollectionToAtomItemTranslator : ITranslator
    {
        private readonly IServiceProvider serviceProvider;
        private AtomRouter atomRouter;

        public StacCollectionToAtomItemTranslator(IServiceProvider serviceProvider)
        {
            atomRouter = new AtomRouter(serviceProvider.GetRequiredService<IResourceServiceProvider>());
            this.serviceProvider = serviceProvider;
            Key = "staccollection-to-atom";
        }

        public int Priority { get; set; }
        public string Key { get; set; }

        public string Label => "STAC Collection to ATOM Entry";

        public string Description => "This translator is able to convert a STAC Collection to an ATOM Entry.";

        public async Task<T> TranslateAsync<T>(IResource node, CancellationToken ct) where T : IResource
        {
            if (typeof(T) != typeof(AtomItemNode)) return default(T);
            if (node is T) return (T)node;
            if (!(node is StacCollectionNode)) return default(T);

            IResource atomItemNode = new AtomItemNode(CreateAtomItem(node as StacCollectionNode), node.Uri);
            return (T)atomItemNode;
        }

        private StarsAtomItem CreateAtomItem(StacCollectionNode stacCollectionNode)
        {
            // First, let's create our atomItem
            StarsAtomItem atomItem = StarsAtomItem.Create(stacCollectionNode.StacCollection, stacCollectionNode.Uri);

            // Add TMS offering via titiler if possible
            TitilerService titilerService = serviceProvider.GetService<TitilerService>();
            bool imageOfferingSet = false;
            if (titilerService != null)
            {
                imageOfferingSet = atomItem.TryAddTitilerOffering(stacCollectionNode, titilerService);
            }

            // Add offering via egms if possible
            EgmsService egmsService = serviceProvider.GetService<EgmsService>();
            if (egmsService != null)
            {
                imageOfferingSet = imageOfferingSet || atomItem.TryAddEGMSOffering(stacCollectionNode, egmsService);
            }

            // Add offering if a web-map link is available
            if (stacCollectionNode.StacCollection.Links != null)
            {
                foreach (StacLink link in stacCollectionNode.StacCollection.Links)
                {
                    // WMS link -> WMS offering
                    if (link.RelationshipType == "wms" && link.Uri != null)
                    {
                        atomItem.AddWMSOffering(link);
                        imageOfferingSet = true;
                    }
                }
            }

            // if no previous image offering set, then let's try a simple overlay offering
            if (!imageOfferingSet)
                atomItem.AddImageOverlayOffering(stacCollectionNode);

            return atomItem;
        }
    }
}
