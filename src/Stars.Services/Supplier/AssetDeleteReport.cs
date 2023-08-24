// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: AssetDeleteReport.cs

using System;
using System.Collections.Generic;

namespace Terradue.Stars.Services.Supplier
{
    public class AssetDeleteReport
    {
        private IDictionary<string, Exception> assetsExceptions;

        public AssetDeleteReport()
        {
            assetsExceptions = new Dictionary<string, Exception>();
        }

        public IDictionary<string, Exception> AssetsExceptions { get => assetsExceptions; set => assetsExceptions = value; }

    }
}
