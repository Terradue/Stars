using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Stac;
using Stars.Interface.Router;

namespace Stars.Interface.Supply.Asset
{
    public interface IAsset : IRoute
    {
        string Label { get; }

        IEnumerable<string> Roles { get; }

        IStreamable GetStreamable();
    }
}