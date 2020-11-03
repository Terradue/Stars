using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Stac;

namespace Terradue.Stars.Interface.Router
{
    public interface IStreamable : IRoute
    {
        bool CanBeRanged { get; }

        Task<Stream> GetStreamAsync();

        

        Task<Stream> GetStreamAsync(long start, long end = -1);
    }
}