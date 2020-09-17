using System.Threading.Tasks;

namespace Stars.Interface.Router.Translator
{
    public interface ITranslator
    {
        Task<T> Translate<T>(INode node) where T : INode;
    }
}