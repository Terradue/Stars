// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IResourceLink.cs

namespace Terradue.Stars.Interface
{
    public interface IResourceLink : IResource
    {
        string Relationship { get; }

        string Title { get; }

    }
}
