﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: TestStreamable.cs

using System;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Terradue.Stars.Interface;

namespace Stars.Tests
{
    internal class TestStreamable : IStreamResource
    {
        private readonly Stream stream;
        private readonly ulong contentLength;

        public TestStreamable(Stream stream, ulong contentLength)
        {
            this.stream = stream;
            this.contentLength = contentLength;
        }

        public bool CanBeRanged => false;

        public ContentType ContentType => new("application/octet-stream");

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => contentLength;

        public ContentDisposition ContentDisposition => new() { FileName = "test.bin" };

        public Uri Uri => new("http://localhost/test.bin");

        public Task<Stream> GetStreamAsync(CancellationToken ct)
        {
            return Task.FromResult(stream);
        }

        public Task<Stream> GetStreamAsync(long start, CancellationToken ct, long end = -1)
        {
            throw new NotImplementedException();
        }
    }
}
