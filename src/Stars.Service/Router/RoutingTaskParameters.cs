using System;
using System.Collections.Generic;
using Stars.Interface.Router;

namespace Stars.Service.Router
{
    public class RoutingTaskParameters
    {
        public int Recursivity { get; set; }
        public bool SkipAssets { get; set; }
        
    }
}