// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: S3TestCollection.cs

using Stars.Tests.TestFixtures;
using Xunit;

namespace Stars.Tests
{

    [CollectionDefinition(nameof(S3TestCollection))]
    public class S3TestCollection : ICollectionFixture<LocalStackFixture>, ICollectionFixture<Logging>
    {

    }

}
