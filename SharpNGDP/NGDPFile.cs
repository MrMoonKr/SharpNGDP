using System.IO;

namespace SharpNGDP
{
    public abstract class NGDPFile
    {
        public NGDPFile(Stream stream)
        {
            BaseStream = stream;
        }

        public Stream BaseStream { get; }
        public virtual Stream GetStream() => BaseStream;

        public abstract void Read();
    }
}
