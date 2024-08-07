﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: ISupplier.cs

using System.Threading;
using System.Threading.Tasks;
using Stac.Api.Interfaces;

namespace Terradue.Stars.Interface.Supplier
{
    public interface ISupplier : IPlugin
    {
        string Id { get; }
        string Label { get; }

        Task<IResource> SearchForAsync(IResource item, CancellationToken ct, string identifierRegex = null);

        Task<IItemCollection> SearchForAsync(ISearchExpression searchExpression, CancellationToken ct);

        Task<object> InternalSearchExpressionAsync(ISearchExpression searchExpression, CancellationToken ct);

        Task<IOrder> Order(IOrderable orderableRoute);
    }
}
