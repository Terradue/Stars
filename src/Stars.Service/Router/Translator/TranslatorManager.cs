using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stars.Interface.Model;
using Stars.Interface.Router;
using Stars.Interface.Router.Translator;

namespace Stars.Service.Router.Translator
{
    public class TranslatorManager : AbstractManager<ITranslator>
    {
        public TranslatorManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public async Task<IStacNode> Translate(INode node)
        {
            Dictionary<ITranslator, IStacNode> translations = new Dictionary<ITranslator, IStacNode>();
            foreach (var translator in Plugins)
            {
                IStacNode translation = await translator.Translate(node);
                if(translation != null)
                    return translation;
            }
            return null;
        }
    }
}