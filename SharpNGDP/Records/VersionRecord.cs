using SharpNGDP.Ribbit.PSV;

namespace SharpNGDP.Records
{
    public class VersionRecord : PSVRecord
    {
        public string Region { get; set; }
        public string BuildConfig { get; set; }
        public string CDNConfig { get; set; }
        public string KeyRing { get; set; }
        public string BuildId { get; set; }
        public string VersionsName { get; set; }
        public string ProductConfig { get; set; }

        public override void Read(string[] header, string[] row)
        {
            Region = row[0];
            BuildConfig = row[1];
            CDNConfig = row[2];
            KeyRing = row[3];
            BuildId = row[4];
            VersionsName = row[5];
            ProductConfig = row[6];
        }
    }
}
