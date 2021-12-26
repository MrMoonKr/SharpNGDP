namespace SharpNGDP
{
    /// <summary>
    /// 웹 서버 컨텍스트 ( 핵심 정보 )
    /// </summary>
    public class NGDPContext
    {
        public string RibbitHost { get; set; }
        public int RibbitPort { get; set; }

        // Prioritized by order, multiple for failover
        public string[] PreferredCDNs { get; set; }

        public string Platform { get; set; }

        public string LocalCache { get; set; }

        public readonly static NGDPContext DefaultContext = new NGDPContext()
        {
            RibbitHost      = "us.version.battle.net",
            RibbitPort      = 1119,

            PreferredCDNs   = new[] { "kr", "us", "eu" },
            Platform        = "Windows",

            LocalCache      = "cache"
        };

        public readonly static NGDPContext s_ContextKr = new NGDPContext()
        {
            RibbitHost      = "kr.version.battle.net",
            RibbitPort      = 1119,

            PreferredCDNs   = new[] { "kr", "us" },
            Platform        = "Windows",

            LocalCache      = "blizzard"
        };
    }
}
