// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: TestBase.cs

using System;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Schema;
using Stac.Schemas;
using Terradue.Stars.Interface;
using Terradue.Stars.Interface.Supplier;
using Terradue.Stars.Services.Model.Stac;
using Terradue.Stars.Services.Plugins;
using Terradue.Stars.Services.Resources;
using Terradue.Stars.Services.Supplier.Carrier;
using Terradue.Stars.Services.ThirdParty.Egms;
using Terradue.Stars.Services.ThirdParty.Titiler;
using Xunit;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Terradue.Data.Tests
{
    public abstract class TestBase
    {
        public ServiceCollection Collection { get; }

        protected IConfigurationRoot configuration;

        protected static StacValidator stacValidator = new(new JSchemaUrlResolver());

        private static readonly Assembly ThisAssembly = typeof(TestBase)
#if NETCOREAPP1_1
        .GetTypeInfo()
#endif
        .Assembly;
        private static readonly string AssemblyName = ThisAssembly.GetName().Name;

        protected TestBase(ITestOutputHelper outputHelper) : this()
        {
            OutputHelper = outputHelper;
            Collection.AddLogging((builder) => builder.AddXUnit(OutputHelper));
            Collection.AddSingleton<CarrierManager, CarrierManager>();
            Collection.AddSingleton<ICarrier, LocalStreamingCarrier>();
            Collection.AddSingleton<IFileSystem, FileSystem>();
            Collection.AddSingleton<IResourceServiceProvider, DefaultResourceServiceProvider>();
            Collection.AddSingleton<StacRouter, StacRouter>();
            Collection.AddSingleton<TitilerService, TitilerService>();
            Collection.AddSingleton<EgmsService, EgmsService>();
            Collection.AddSingleton<IVectorService, TestVectorService>();
        }

        protected TestBase()
        {
            Collection = new ServiceCollection();
            Collection.AddLogging();
            Collection.AddSingleton<IResourceServiceProvider, DefaultResourceServiceProvider>();
            Collection.AddSingleton<CarrierManager, CarrierManager>();
            Collection.AddSingleton<ICarrier, LocalStreamingCarrier>();
            Collection.AddSingleton<IFileSystem, FileSystem>();
            Collection.AddSingleton<StacRouter, StacRouter>();
            Collection.AddSingleton<TitilerService, TitilerService>();
            Collection.AddSingleton<EgmsService, EgmsService>();
            Collection.AddSingleton<IVectorService, TestVectorService>();
            Collection.AddHttpClient();
            var builder = new ConfigurationBuilder();
            // tell the builder to look for the appsettings.json file
            var configFile = new FileInfo(Path.Join(@"../../../../Stars.Data", "stars-data.json"));
            configFile.OpenRead();
            builder.AddNewtonsoftJsonFile(configFile.FullName, optional: false, reloadOnChange: false);
            configFile = new FileInfo(Path.Join(@"../../../", "testsettings.json"));
            configFile.OpenRead();
            builder.AddNewtonsoftJsonFile(configFile.FullName, optional: false, reloadOnChange: false);
            configuration = builder.Build();
            LoadPlugins();
        }

        private void LoadPlugins()
        {
            Collection.Configure<PluginsOptions>(configuration.GetSection("Plugins"));
            PluginManager pluginManager = ActivatorUtilities.CreateInstance<PluginManager>(ServiceProvider);
            pluginManager.LoadPlugins(Collection, s => AssemblyLoadContext.Default);
        }



        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = ThisAssembly.Location;
                return Path.GetDirectoryName(codeBase);
            }
        }

        protected string GetJson(string folder, [CallerMemberName] string name = null)
        {
            var type = GetType().Name;
            var path = Path.Combine(AssemblyDirectory, @"../../..", "Resources", folder, type + "_" + name + ".json");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("file not found at " + path);
            }

            return File.ReadAllText(path);
        }

        protected void WriteJson(string folder, string content, [CallerMemberName] string name = null)
        {
            var type = GetType().Name;
            var path = Path.Combine(AssemblyDirectory, @"../../..", "Resources", folder, type + "_" + name + ".json");

            File.WriteAllText(path, content);
        }

        protected Uri GetUseCaseFileUri(string name)
        {
            var type = GetType().Name;
            var path = Path.Combine(AssemblyDirectory, @"../../..", "Resources", type, name);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("file not found at " + path);
            }

            return new Uri(path);
        }

        protected string GetResourceFilePath(string name)
        {
            var path = Path.Combine(AssemblyDirectory, @"../../..", "Resources", name);

            return path;
        }

        public bool ValidateJson(string jsonstr)
        {
            return stacValidator.ValidateJson(jsonstr);
        }

        protected IServiceProvider ServiceProvider => Collection.BuildServiceProvider();

        public IConfigurationRoot Configuration { get => configuration; }
        public ITestOutputHelper OutputHelper { get; }
    }
}
