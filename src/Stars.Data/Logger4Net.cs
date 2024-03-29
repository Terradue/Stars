using log4net;
using log4net.Repository.Hierarchy;
using log4net.Core;
using log4net.Appender;
using log4net.Layout;
using System.Reflection;

namespace Terradue.Stars.Data
{
    public class Logger4Net
    {
        public static bool Configured = false;

        public static void Setup(Microsoft.Extensions.Logging.ILogger logger)
        {
            if ( Configured ) return;

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(Assembly.GetAssembly(typeof(Logger4Net)));

            Log4Net2LoggerAppender appender = new Log4Net2LoggerAppender(logger);

            hierarchy.Root.AddAppender(appender);

            hierarchy.Root.Level = Level.Debug;
            hierarchy.Configured = true;

            Configured = true;
        }
    }
}