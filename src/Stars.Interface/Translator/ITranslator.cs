using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Router.Translator
{
    public interface ITranslator : IPlugin
    {
        string Label { get; }

        Task<T> Translate<T>(IResource node) where T : IResource;
    }
}