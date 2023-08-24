// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IResource.cs

using System.Net.Mime;

namespace Terradue.Stars.Interface
{
    public interface IResource : ILocatable
    {
        ContentType ContentType { get; }
        ResourceType ResourceType { get; }
        ulong ContentLength { get; }
        ContentDisposition ContentDisposition { get; }
    }
}
