// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IDeliveryQuotation.cs

using System;
using System.Collections.Generic;
using System.Linq;

namespace Terradue.Stars.Interface.Supplier
{
    public interface IDeliveryQuotation
    {
        IDictionary<string, IOrderedEnumerable<IDelivery>> AssetsDeliveryQuotes { get; }

        IDictionary<string, Exception> AssetsExceptions { get; }

    }
}
