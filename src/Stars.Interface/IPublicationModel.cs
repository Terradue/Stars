// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IPublicationModel.cs

using System.Collections.Generic;
using Stac;

namespace Terradue.Stars.Interface
{
    public interface IPublicationModel
    {
        string Url { get; }

        string CatalogId { get; }

        List<StacLink> AdditionalLinks { get; }

        List<ISubject> Subjects { get; }

        string Collection { get; }

        int Depth { get; }

        List<string> AssetsFilters { get; }
    }
}
