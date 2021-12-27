using System;
using System.Diagnostics;
using System.IO;

namespace SharpNGDP
{
    /// <summary>
    /// 응답 추상화
    /// </summary>
    public abstract class NGDPResponse
    {
        private static Logger log = Logger.Create<NGDPResponse>();

        protected NGDPResponse( NGDPRequest request , Stream stream )
        {
            Request     = request;
            Received    = DateTime.UtcNow;
            using ( var ms = new MemoryStream() )
            {
                stream.CopyTo( ms );
                Buffer  = ms.ToArray();
            }
            log.WriteLine( $"Received response from request to {Request.URI} at {Received} of {Buffer.Length} bytes" );
        }

        public NGDPRequest      Request { get; }
        public DateTime         Received { get; }
        public byte[]           Buffer { get; }

        /// <summary>
        /// 캐시된 Buffer에 대한 메모리스트림 반환
        /// </summary>
        /// <returns></returns>
        public Stream GetStream() => new MemoryStream( Buffer );

        public T GetFile<T>() where T : NGDPFile
        {
            var sw = Stopwatch.StartNew();

            var file = (T)Activator.CreateInstance( typeof(T), GetStream() );
            file.Read();

            sw.Stop();
            log.WriteLine( $"Parsing for GetFile took { sw.Elapsed }" );

            return file;
        }
    }
}
