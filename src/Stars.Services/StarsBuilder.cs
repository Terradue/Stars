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
