// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: ConventionTestBase.cs

using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using Xunit.Abstractions;

namespace Stars.Console.Tests
{
    public class ConventionTestBase
    {
        protected readonly ITestOutputHelper _output;

        protected ConventionTestBase(ITestOutputHelper output)
        {
            _output = output;
        }

        protected CommandLineApplication<T> Create<T, TConvention>()
            where T : class
            where TConvention : IConvention, new()
        {
            var app = new CommandLineApplication<T>(new TestConsole(_output));
            app.Conventions.AddConvention(new TConvention());
            return app;
        }
    }
}
