// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IStarsBuilder.cs

using Microsoft.Extensions.DependencyInjection;

namespace Terradue.Stars.Services
{
    //
    // Summary:
    //     An interface for configuring Stars.
    //
    public interface IStarsBuilder
    {
        // Summary:
        //     Gets the Microsoft.Extensions.DependencyInjection.IServiceCollection where Stars
        //     services are configured.
        IServiceCollection Services { get; }
    }
}
