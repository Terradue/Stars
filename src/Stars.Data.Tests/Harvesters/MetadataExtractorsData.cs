// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: MetadataExtractorsData.cs

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Terradue.Stars.Data.Model.Metadata;
using Terradue.Stars.Interface.Processing;

namespace Terradue.Data.Tests.Harvesters
{
    public class MetadataExtractorsData : TestBase, IEnumerable<object[]>
    {
        public MetadataExtractorsData() : base()
        {
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IDictionary<string, string[]> GetHarvesterResourcePaths()
        {
            return configuration.GetSection("Tests:Harvesters").Get<IDictionary<string, string[]>>();
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (IProcessing processing in ServiceProvider.GetService<IEnumerable<IProcessing>>())
            {
                if (typeof(MetadataExtraction).IsAssignableFrom(processing.GetType()))
                {
                    var paths = GetHarvesterResourcePaths();
                    if (!paths.ContainsKey(processing.Key))
                        continue;
                    foreach (var path in paths[processing.Key])
                    {
                        foreach (var subdir in Directory.GetDirectories(GetResourceFilePath(path), "data", new EnumerationOptions() { RecurseSubdirectories = true }))
                        {
                            foreach (var datadir in Directory.GetDirectories(subdir, "*", new EnumerationOptions() { RecurseSubdirectories = false }))
                            {
                                yield return new object[] { Path.GetFileName(datadir), datadir + "/", processing };
                            }
                        }
                    }
                }
            }
        }
    }
}
