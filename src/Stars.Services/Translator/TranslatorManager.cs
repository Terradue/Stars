using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
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

        public async Task<T> TranslateAsync<T>(IResource node, CancellationToken ct) where T : IResource
        {
            Dictionary<ITranslator, T> translations = new Dictionary<ITranslator, T>();
            if (node is T) return (T)node;
            List<Exception> exceptions = new List<Exception>();
            foreach (var translator in GetPlugins().Values)
            {
                try
                {
                    T translation = await translator.TranslateAsync<T>(node, ct);
                    if (translation != null)
                        return translation;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
            throw new AggregateException(exceptions);
        }
    }
}