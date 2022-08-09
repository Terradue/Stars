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