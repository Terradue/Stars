using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Stars.Supplier.Catalog
{
    public class LocalCatalogGeneratorManager
    {
        private IEnumerable<ILocalCatalogGenerator> _generators;

        public LocalCatalogGeneratorManager(IEnumerable<ILocalCatalogGenerator> generators)
        {
            this._generators = generators;
        }

        internal static IEnumerable<Type> LoadCatalogGeneratorPlugins(string[] pluginPaths)
        {
            return pluginPaths.SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = Assembly.Load(pluginPath);
                return CreateResourceRouter(pluginAssembly);
            });
        }

        internal static IEnumerable<Type> CreateResourceRouter(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetInterface(typeof(ILocalCatalogGenerator).FullName) != null)
                {
                    yield return type;
                }
            }
        }

        internal ILocalCatalogGenerator GetLocalCatalogGenerator(string format)
        {
            return _generators.FirstOrDefault(r => r.Id == format);
        }
    }
}