// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: SuppliersTestsData.cs

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Data.Suppliers;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Translator;

namespace Terradue.Data.Tests.Suppliers
{
    public class SuppliersTestsData : TestBase, IEnumerable<object[]>
    {
        public SuppliersTestsData() : base()
        {
            Collection.AddSingleton<ISupplier>(sp =>
            {
                var supplier = new OpenSearchableSupplier(sp.GetRequiredService<ILogger<OpenSearchableSupplier>>(),
                                                          sp.GetRequiredService<TranslatorManager>());
                supplier.Key = "OpenSearchable";
                return supplier;
            });
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IDictionary<string, string[]> GetSuppliersResourcePaths()
        {
            return configuration.GetSection("Tests:Suppliers").Get<IDictionary<string, string[]>>();
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (ISupplier supplier in ServiceProvider.GetService<IEnumerable<ISupplier>>())
            {
                var paths = GetSuppliersResourcePaths();
                if (!paths.ContainsKey(supplier.Key))
                    continue;
                foreach (var path in paths[supplier.Key])
                {
                    foreach (var file in Directory.GetFiles(GetResourceFilePath("Suppliers/" + path), "*.json", new EnumerationOptions() { RecurseSubdirectories = true }))
                    {
                        yield return new object[] { supplier.Key, supplier, file };
                    }
                }
            }
        }
    }
}
