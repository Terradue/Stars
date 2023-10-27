using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using Terradue.Stars.Data.Model.Metadata;
using System.Reflection;
using Terradue.Stars.Interface.Processing;
using Xunit.Abstractions;
using System.IO;
using Microsoft.Extensions.Configuration;
using Terradue.Stars.Interface.Supplier;

namespace Terradue.Data.Tests.Suppliers
{
    public class SuppliersTestsData : TestBase, IEnumerable<object[]>
    {
        public SuppliersTestsData() : base()
        {
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
                    foreach (var file in Directory.GetFiles(GetResourceFilePath(path), "*.json", new EnumerationOptions() { RecurseSubdirectories = true }))
                    {
                        yield return new object[] { supplier.Key, supplier, file };
                    }
                }
            }
        }
    }
}
