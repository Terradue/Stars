using System;
using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Router
{
    public interface IResourceServiceProvider
    {
        public Task<IResource> CreateAsync(Uri url);
    }
}