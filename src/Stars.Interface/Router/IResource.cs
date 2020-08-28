using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;

namespace Stars.Interface.Router
{
    public interface IResource: IRoute
    {
        string Label { get; }
        string Id { get; }
        string ReadAsString();
        Stream GetAsStream();

    }
}