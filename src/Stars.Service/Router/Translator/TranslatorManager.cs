using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stars.Interface.Router;
using Stars.Interface.Router.Translator;

namespace Stars.Service.Router.Translator
{
    public class TranslatorManager : AbstractManager<ITranslator>
    {
        public TranslatorManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public async Task<T> Translate<T>(INode node) where T : INode
        {
            Dictionary<ITranslator, T> translations = new Dictionary<ITranslator, T>();
            foreach (var translator in Plugins)
            {
                T translation = await translator.Translate<T>(node);
                if(translation != null)
                    return translation;
            }
            return default(T);
        }
    }
}