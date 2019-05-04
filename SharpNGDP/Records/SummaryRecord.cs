using SharpNGDP.Ribbit.PSV;

namespace SharpNGDP.Records
{
    public class SummaryRecord : PSVRecord
    {
        public string Product { get; set; }
        public string Seqn { get; set; }
        public string Flags { get; set; }

        public override void Read(string[] header, string[] row)
        {
            Product = row[0];
            Seqn = row[1];
            Flags = row[2];
        }
    }
}
