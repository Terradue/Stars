﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: GenericAsset.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Terradue.Stars.Interface;

namespace Terradue.Stars.Services.Supplier
{
    public class GenericAsset : IAsset
    {
        private readonly IResource route;
        private readonly string title;
        private readonly IReadOnlyList<string> roles;
        private Uri uri;
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public GenericAsset(IResource route, string title, IReadOnlyList<string> roles)
        {
            this.route = route;
            this.title = title;
            this.roles = roles;
            uri = route.Uri;
            if (route is IAsset)
            {
                properties = new Dictionary<string, object>((route as IAsset).Properties.ToDictionary(x => x.Key, x => x.Value));
            }
        }

        public string Title => title;

        public IReadOnlyList<string> Roles => roles;

        public Uri Uri
        {
            get
            {
                return uri;
            }
            private set
            {
                uri = value;
            }
        }

        public ContentType ContentType => route.ContentType;

        public ResourceType ResourceType => ResourceType.Asset;

        public ulong ContentLength => route.ContentLength;

        public ContentDisposition ContentDisposition => route.ContentDisposition;

        public IReadOnlyDictionary<string, object> Properties => properties;

        public IEnumerable<IAsset> Alternates => new IAsset[] { };

        internal void MergeProperties(IReadOnlyDictionary<string, object> props)
        {
            foreach (var key in props.Keys)
            {
                properties.Remove(key);
                properties.Add(key, props[key]);
            }
        }

    }
}
