using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stars.Interface.Supply;

namespace Stars.Interface.Router
{
    public interface INode : IRoute
    {
        string Label { get; }
        string Id { get; }
        bool IsCatalog { get; }
        Stream GetStream();
        string ReadAsString();
    }
}