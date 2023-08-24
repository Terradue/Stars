// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IPublicationState.cs

using Stac;

namespace Terradue.Stars.Interface
{
    public interface IPublicationState
    {
        StacLink GetPublicationLink();
    }
}
