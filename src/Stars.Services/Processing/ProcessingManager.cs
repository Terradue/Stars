﻿// Copyright (c) by Terradue Srl. All Rights Reserved.
// License under the AGPL, Version 3.0.
// File Name: ProcessingManager.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Terradue.Stars.Interface.Processing;

namespace Terradue.Stars.Services.Processing
{
    public class ProcessingManager : AbstractManager<IProcessing>
    {

        public ProcessingManager(ILogger<ProcessingManager> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
        {
        }

        public IEnumerable<IProcessing> GetProcessings(ProcessingType processingType)
        {
            return GetPlugins().Values.Where(p => p.ProcessingType == processingType);
        }



    }
}
