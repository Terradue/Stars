using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;
using Terradue.Stars.Interface.Supply;

namespace Terradue.Stars.Interface.Router
{
    public interface IStreamable : IRoute
    {
        Task<Stream> GetStreamAsync();
        
        ContentDisposition ContentDisposition { get; }
    }
}