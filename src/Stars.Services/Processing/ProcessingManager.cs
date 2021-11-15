using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terradue.Stars.Interface.Processing;

namespace Terradue.Stars.Services.Processing
{
    public class ProcessingManager : AbstractManager<IProcessing>
    {

        public ProcessingManager(ILogger<ProcessingManager> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public IEnumerable<IProcessing> GetProcessings(ProcessingType processingType){
            return GetPlugins().Values.Where(p => p.ProcessingType == processingType);
        }

        

    }
}