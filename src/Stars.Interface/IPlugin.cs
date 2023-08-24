// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IPlugin.cs

namespace Terradue.Stars.Interface
{
    public interface IPlugin
    {
        int Priority { get; set; }
        string Key { get; set; }

    }
}
