using SharpNGDP.Ribbit.PSV;

namespace SharpNGDP.Records
{
    /// <summary>
    /// 빌드 버전에 따른 파일 이름 해시 ( HEX:16 ) 정보.
    /// </summary>
    public class VersionRecord : PSVRecord
    {
        public string Region { get; set; }
        public string BuildConfig { get; set; }
        public string CDNConfig { get; set; }
        public string KeyRing { get; set; }
        public string BuildId { get; set; }
        public string VersionsName { get; set; }
        public string ProductConfig { get; set; }

        /// <summary>
        /// 레코드 파싱
        /// </summary>
        /// <param name="header">칼럼 항목</param>
        /// <param name="row">레코드</param>
        public override void Read( string[] header, string[] row )
        {
            Region              = row[0];
            BuildConfig         = row[1];
            CDNConfig           = row[2];
            KeyRing             = row[3];
            BuildId             = row[4];
            VersionsName        = row[5];
            ProductConfig       = row[6];
        }
    }
}
