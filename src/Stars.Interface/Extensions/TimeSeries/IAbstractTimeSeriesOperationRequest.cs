// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: IAbstractTimeSeriesOperationRequest.cs

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Extensions.TimeSeries
{
    public interface IAbstractTimeSeriesOperationRequest<T>
    {
        string NamespaceId { get; }

        string CollectionId { get; }

        Task<T> GetOperationPayloadAsync(CancellationToken cancellationToken);

        IReadOnlyDictionary<string, string> Properties { get; }
    }
}
