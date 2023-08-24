// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StarsCollection.cs

using Xunit;

namespace Stars.Tests
{
    [CollectionDefinition(nameof(StarsCollection))]
    public class StarsCollection : ICollectionFixture<Logging>
    {
    }
}
