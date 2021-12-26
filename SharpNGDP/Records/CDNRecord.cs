using SharpNGDP.Ribbit.PSV;

namespace SharpNGDP.Records
{
    /// <summary>
    /// 파일 서버 및 경로 루트 정보.
    /// </summary>
    public class CDNRecord : PSVRecord
    {
        /// <summary>
        /// 리전. kr, us, eu, ...
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// "tpr/wow", "", ...
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// "level3.blizzard.com", "kr.cdn.blizzard.com", ...
        /// </summary>
        public string[] Hosts { get; set; }
        /// <summary>
        /// "http://blizzard.gcdn.cloudn.co.kr/?maxhosts=4", ...
        /// </summary>
        public string[] Servers { get; set; }
        /// <summary>
        /// "tpr/configs/data", ...
        /// </summary>
        public string ConfigPath { get; set; }

        /// <summary>
        /// 레코드 읽기
        /// </summary>
        /// <param name="header"></param>
        /// <param name="row"></param>
        public override void Read( string[] header, string[] row )
        {
            Name            = row[0];
            Path            = row[1];
            Hosts           = row[2].Split(' ');
            Servers         = row[3].Split(' ');
            ConfigPath      = row[4];
        }
    }
}
