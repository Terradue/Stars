using System;
using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface
{
    public interface ICatalogService
    {
        Task<Uri> PublishAsync(IPublicationModel publicationModel, CancellationToken cancellationToken);
    }
}