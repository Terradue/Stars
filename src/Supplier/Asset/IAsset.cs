using System;
using System.Net.Mime;

namespace Stars.Supplier.Asset
{
    public interface IAsset
    {
        Uri Uri { get; }
        ContentType ContentType { get; }
        long ContentLength { get; }
        string Label { get; }
    }
}