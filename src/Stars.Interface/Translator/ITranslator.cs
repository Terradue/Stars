using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Router.Translator
{
    public interface ITranslator : IPlugin
    {
        string Label { get; }

        string Description { get; }

        Task<T> TranslateAsync<T>(IResource node, CancellationToken ct) where T : IResource;
    }
}