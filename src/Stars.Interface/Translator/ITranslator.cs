using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Router.Translator
{
    public interface ITranslator : IPlugin
    {
        Task<T> Translate<T>(IRoute node) where T : IRoute;
    }
}