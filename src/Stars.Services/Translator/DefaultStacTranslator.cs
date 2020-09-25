using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Router.Translator;
using Terradue.Stars.Services.Model.Atom;
using Stac.Item;
using Terradue.Stars.Services.Model.Stac;
using Stac.Catalog;
using System.Collections.Generic;
using Stac;

namespace Terradue.Stars.Services.Translator
{
    public class DefaultStacTranslator : ITranslator
    {
        private ILogger logger;

        public DefaultStacTranslator(ILogger logger)
        {
            this.logger = logger;
        }

        public Task<T> Translate<T>(INode node) where T : INode
        {
            if (!(node is IRoutable)) return Task.FromResult<T>(default(T));
            if (node.IsCatalog)
            {
                return Task.FromResult<T>((T)CreateStacCatalogNode(node));
            }
            else
            {
                return Task.FromResult<T>((T)CreateStacItemNode(node));
            }
        }

        private INode CreateStacCatalogNode(INode node)
        {
            StacCatalog catalog = new StacCatalog(node.Id, node.Label, CreateStacLinks(node));
            return new StacCatalogNode(catalog);
        }

        private IEnumerable<StacLink> CreateStacLinks(INode node)
        {
            return null;
        }

        private INode CreateStacItemNode(INode node)
        {
            StacItem stacItem = new StacItem(null, new Dictionary<string, object>(), node.Id);
            return new StacItemNode(stacItem);
        }

        public static DefaultStacTranslator Create(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {
            return new DefaultStacTranslator((ILogger)serviceProvider.GetService(typeof(ILogger)));
        }

        public void Configure(IConfigurationSection configurationSection, IServiceProvider serviceProvider)
        {
        
        }
    }
}
