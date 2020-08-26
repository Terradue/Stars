using System;
using System.Net.Mime;
using Stars.Router;

namespace Stars.Supply.Asset
{
    public interface IAsset : IRoute
    {
        string Label { get; }
    }
}