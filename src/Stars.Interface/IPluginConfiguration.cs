// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IPluginConfiguration.cs

namespace Terradue.Stars.Interface
{
    public interface IPluginOption
    {
        string Type { get; }
        int? Priority { get; }
    }
}
