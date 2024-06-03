
using log4net.Appender;
using log4net.Core;
using Microsoft.Extensions.Logging;

namespace Terradue.Stars.Data
{
    internal class Log4Net2LoggerAppender : AppenderSkeleton
    {
        private Microsoft.Extensions.Logging.ILogger logger;

        public Log4Net2LoggerAppender(Microsoft.Extensions.Logging.ILogger logger)
        {
            this.logger = logger;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            switch (loggingEvent.Level.Name)
            {
                case "DEBUG":
                    logger.LogDebug(loggingEvent.RenderedMessage);
                    break;
            }
        }
    }
}
