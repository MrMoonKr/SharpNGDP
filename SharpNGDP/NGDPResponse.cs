using System;
using System.IO;

namespace SharpNGDP
{
    public abstract class NGDPResponse
    {
        protected NGDPResponse(NGDPRequest request, Stream stream)
        {
            Request = request;
            Received = DateTime.UtcNow;
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                Buffer = ms.ToArray();
            }
        }

        public NGDPRequest Request { get; }
        public DateTime Received { get; }
        public byte[] Buffer { get; }

        public Stream GetStream() => new MemoryStream(Buffer);

        public T GetFile<T>() where T : NGDPFile
        {
            var file = (T)Activator.CreateInstance(typeof(T), GetStream());
            file.Read();
            return file;
        }
    }
}
