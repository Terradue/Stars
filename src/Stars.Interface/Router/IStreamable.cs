using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Stars.Interface.Supply;

namespace Stars.Interface.Router
{
    public interface IStreamable : IRoute
    {
        Task<Stream> GetStreamAsync();
        ContentDisposition ContentDisposition { get; }
    }
}