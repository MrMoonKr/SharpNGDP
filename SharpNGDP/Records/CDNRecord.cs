using SharpNGDP.Ribbit.PSV;

namespace SharpNGDP.Records
{
    public class CDNRecord : PSVRecord
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string[] Hosts { get; set; }
        public string[] Servers { get; set; }
        public string ConfigPath { get; set; }

        public override void Read(string[] header, string[] row)
        {
            Name = row[0];
            Path = row[1];
            Hosts = row[2].Split(' ');
            Servers = row[3].Split(' ');
            ConfigPath = row[4];
        }
    }
}
