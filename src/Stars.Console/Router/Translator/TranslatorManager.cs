using System.Collections.Generic;
using System.Threading.Tasks;
using Stars.Interface.Router;
using Stars.Interface.Router.Translator;

namespace Stars
{
    internal class TranslatorManager : AbstractManager<ITranslator>
    {
        public TranslatorManager(IEnumerable<ITranslator> translators) : base(translators)
        {
        }

        internal async Task<IDictionary<ITranslator, INode>> Translate(INode node)
        {
            Dictionary<ITranslator, INode> translations = new Dictionary<ITranslator, INode>();
            foreach (var translator in _items)
            {
                INode translation = await translator.Translate(node);
                if(translation != null)
                    translations.Add(translator, translation);
            }
            return translations;
        }
    }
}