// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StacItemToAtomItemTranslator.cs

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
using Terradue.Stars.Services.ThirdParty.Titiler;

namespace Terradue.Stars.Data.Translators
{
    public class StacItemToAtomItemTranslator : ITranslator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly AtomRouter atomRouter;

        public StacItemToAtomItemTranslator(IServiceProvider serviceProvider)
        {
            atomRouter = new AtomRouter(serviceProvider.GetRequiredService<IResourceServiceProvider>());
            this.serviceProvider = serviceProvider;
            Key = "stac-to-atom";
        }

        public int Priority { get; set; }
        public string Key { get; set; }

        public string Label => "STAC Item to ATOM Entry";

        public async Task<T> TranslateAsync<T>(IResource node, CancellationToken ct) where T : IResource
        {
            if (typeof(T) != typeof(AtomItemNode)) return default(T);
            if (node is T nodeT) return nodeT;
            if (!(node is StacItemNode)) return default(T);

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
                atomItem.AddImageOverlayOffering(stacItemNode);

            // Try to add vector offering
            IVectorService vectorService = serviceProvider.GetService<IVectorService>();
            if (vectorService != null)
            {
                atomItem.TryAddVectorOffering(stacItemNode, vectorService);
            }

            // Add offering if a web-map link is available
            if (stacItemNode.StacItem.Links != null)
            {
                foreach (StacLink link in stacItemNode.StacItem.Links)
                {
                    // WMS link -> WMS offering
                    if (link.RelationshipType == "wms" && link.Uri != null)
                    {
                        atomItem.AddWMSOffering(link);
                    }
                    // XYZ link -> TWM offering
                    if (link.RelationshipType == "xyz" && link.Uri != null)
                    {
                        atomItem.AddTWMOffering(link);
                    }
                }
            }

            return atomItem;
        }
    }
}
