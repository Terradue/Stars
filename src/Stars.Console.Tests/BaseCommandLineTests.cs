using Terradue.Stars.Console;
using Xunit;
using Xunit.Abstractions;

namespace Stars.Console.Tests
{
    public class BaseCommandLineTests
    {
        private readonly ITestOutputHelper _output;

        public BaseCommandLineTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestNoArguments()
        {
            var console = new TestConsole(_output);
            var app = StarsApp.CreateApplication(console);
            Assert.Equal(0, app.Execute());
            Assert.Contains("Stars " + typeof(StarsApp).Assembly.GetName().Version.ToString(3), console.Output.ToString());
            Assert.Contains("Usage: Stars [command] [options]", console.Output.ToString());
        }

        [Fact]
        public void ListPlugins()
        {
            var console = new TestConsole(_output);
            var app = StarsApp.CreateApplication(console);
            Assert.Equal(0, app.Execute("plugins"));
            Assert.Contains("Stars\n" + typeof(StarsApp).Assembly.GetName().Version.ToString(3), console.Output.ToString());
        }
    }
}
