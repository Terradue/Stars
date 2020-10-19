using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Router;
using Terradue.Stars.Interface.Router.Translator;

namespace Terradue.Stars.Services.Translator
{
    public class TranslatorManager : AbstractManager<ITranslator>
    {
        public TranslatorManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public async Task<T> Translate<T>(IRoute node) where T : IRoute
        {
            Dictionary<ITranslator, T> translations = new Dictionary<ITranslator, T>();
            foreach (var translator in Plugins)
            {
                T translation = await translator.Value.Translate<T>(node);
                if(translation != null)
                    return translation;
            }
            return default(T);
        }
    }
}