using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Terradue.Stars.Services.Processing
{
    public class ProcessingManager : AbstractManager<IProcessing>
    {

        public ProcessingManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

    }
}