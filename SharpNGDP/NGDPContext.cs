namespace SharpNGDP
{
    /// <summary>
    /// 서버 컨텍스트 ( 핵심 정보 )
    /// https://wowdev.wiki/Ribbit
    /// </summary>
    public class NGDPContext
    {
        public string       RibbitHost { get; set; }
        public int          RibbitPort { get; set; }
        public string[]     PreferredCDNs { get; set; }
        public string       Platform { get; set; }
        public string       LocalCache { get; set; }

        /// <summary>
        /// 미국 지역
        /// </summary>
        public readonly static NGDPContext s_ContextDefault = new NGDPContext()
        {
            RibbitHost      = "us.version.battle.net",
            RibbitPort      = 1119,

            PreferredCDNs   = new[] { "kr", "us", "eu" },
            Platform        = "Windows",

            LocalCache      = "cache"
        };

        /// <summary>
        /// 한국 지역
        /// </summary>
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
