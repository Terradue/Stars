// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: StarsBuilder.cs

using Microsoft.Extensions.DependencyInjection;

namespace Terradue.Stars.Services
{
    internal class StarsBuilder : IStarsBuilder
    {
        private IServiceCollection services;

        public StarsBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public IServiceCollection Services => services;
    }
}
