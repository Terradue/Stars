using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Router.Translator;

namespace Terradue.Stars.Services.Translator
{
    public class TranslatorManager : AbstractManager<ITranslator>
    {
        public TranslatorManager(ILogger<TranslatorManager> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public async Task<T> Translate<T>(IResource node) where T : IResource
        {
            Dictionary<ITranslator, T> translations = new Dictionary<ITranslator, T>();
            if (node is T) return (T)node;
            foreach (var translator in Plugins.Values)
            {
                try
                {
                    T translation = await translator.Translate<T>(node);
                    if (translation != null)
                        return translation;
                }
                catch { }
            }
            return default(T);
        }
    }
}