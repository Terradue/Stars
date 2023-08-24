// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: ITranslator.cs

using System.Threading;
using System.Threading.Tasks;

namespace Terradue.Stars.Interface.Router.Translator
{
    public interface ITranslator : IPlugin
    {
        string Label { get; }

        Task<T> TranslateAsync<T>(IResource node, CancellationToken ct) where T : IResource;
    }
}
