using System.Threading.Tasks;
using Stars.Interface.Model;

namespace Stars.Interface.Router.Translator
{
    public interface ITranslator
    {
        Task<IStacNode> Translate(INode node);
    }
}