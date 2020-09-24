using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Terradue.Stars.Service.Supply.Receipt
{
    public class ReceiptManager : AbstractManager<IReceiptAction>
    {

        public ReceiptManager(ILogger logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

    }
}