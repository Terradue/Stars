using System;
using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Stars.Services.Router
{
    public interface IResourceServiceProvider
    {
        public Task<IResource> CreateAsync(Uri url);
    }
}