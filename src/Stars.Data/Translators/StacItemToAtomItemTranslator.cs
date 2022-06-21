using System.Threading.Tasks;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Services.Model.Atom;
using Stac;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Interface;
using System.Net;
using Terradue.Stars.Data.Model.Atom;
using System;
using Terradue.Stars.Services.Store;
using Terradue.Stars.Services.ThirdParty.Titiler;
using Microsoft.Extensions.DependencyInjection;

namespace Terradue.Stars.Data.Translators
{
    public class StacItemToAtomItemTranslator : ITranslator
    {
        private readonly IServiceProvider serviceProvider;
        private AtomRouter atomRouter;
        
        public StacItemToAtomItemTranslator(IServiceProvider serviceProvider)
        {
            this.atomRouter = new AtomRouter(serviceProvider.GetRequiredService<IResourceServiceProvider>());
            this.serviceProvider = serviceProvider;
            Key = "stac-to-atom";
        }

        public int Priority { get; set; }
        public string Key { get; set; }

        public string Label => "STAC Item to ATOM Entry";

        public async Task<T> Translate<T>(IResource node) where T : IResource
        {
            if ( typeof(T) != typeof(AtomItemNode) ) return default(T);
            if ( node is T ) return (T)node;
            if ( !(node is StacItemNode) ) return default(T);

            IResource atomItemNode = new AtomItemNode(CreateAtomItem(node as StacItemNode), node.Uri);
            return (T)atomItemNode;
        }

        private StarsAtomItem CreateAtomItem(StacItemNode stacItemNode)
        {
            // First, let's create our atomItem
            StarsAtomItem atomItem = StarsAtomItem.Create(stacItemNode.StacItem, stacItemNode.Uri);

            // Add EO Profile if possible
            atomItem.TryAddEarthObservationProfile(stacItemNode.StacItem);

            // Add TMS offering via titiler if possible
            TitilerService titilerService = serviceProvider.GetService<TitilerService>();
            bool imageOfferingSet = false;
            if (titilerService != null)
            {
                imageOfferingSet = atomItem.TryAddTitilerOffering(stacItemNode, titilerService);
            }

            // if no previous image offering set, then let's try a simple overlay offering
            if (!imageOfferingSet)
                atomItem.AddImageOverlayOffering(stacItemNode, titilerService);

            return atomItem;
        }
    }
}
