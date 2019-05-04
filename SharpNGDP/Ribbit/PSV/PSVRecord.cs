namespace SharpNGDP.Ribbit.PSV
{
    public abstract class PSVRecord
    {
        public abstract void Read(string[] header, string[] row);
    }
}
