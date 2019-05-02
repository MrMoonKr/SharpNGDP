using System;
using System.Collections.Generic;
using System.Text;

namespace SharpNGDP
{
    public class NGDPContext
    {
        public string RibbitHost { get; set; }
        public int RibbitPort { get; set; }

        // Prioritized by order, multiple for failover
        public string[] PreferredCDNs { get; set; }

        public readonly static NGDPContext DefaultContext = new NGDPContext()
        {
            RibbitHost = "us.version.battle.net",
            RibbitPort = 1119,

            PreferredCDNs = new[] { "eu", "us" }
        };
    }
}
