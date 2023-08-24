// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: ICatalogService.cs

using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface
{
    public interface ICatalogService
    {
        Task<IPublicationState> PublishAsync(IPublicationModel publicationModel, CancellationToken cancellationToken);
    }
}
