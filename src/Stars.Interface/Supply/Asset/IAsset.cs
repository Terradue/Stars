using System;
using System.Net.Mime;
using Stars.Interface.Router;

namespace Stars.Interface.Supply.Asset
{
    public interface IAsset : IRoute
    {
        string Label { get; }
    }
}