using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Stac;
using Terradue.Stars.Interface.Router;

namespace Terradue.Stars.Interface.Supply.Asset
{
    public interface IAsset : IRoute
    {
        string Label { get; }

        IEnumerable<string> Roles { get; }

        IStreamable GetStreamable();
    }
}