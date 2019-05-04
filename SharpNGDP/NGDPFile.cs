using System.IO;

namespace SharpNGDP
{
    public abstract class NGDPFile
    {
        public NGDPFile(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; }

        public abstract void Read();
    }
}
