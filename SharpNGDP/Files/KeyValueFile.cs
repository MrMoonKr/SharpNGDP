using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SharpNGDP.Files
{
    /// <summary>
    /// Key = Value 형태의 설정값들이 저장된 파일.
    /// '#'으로 시작되는 라인은 주석에 사용.
    /// </summary>
    public class KeyValueFile : NGDPFile
    {
        private static readonly Logger log = Logger.Create<KeyValueFile>();

        public KeyValueFile( Stream stream )
            : base( stream )
        { }

        /// <summary>
        /// 설정값 룩업테이블
        /// </summary>
        public Dictionary<string , string> Dictionary { get; private set; }

        public override void Read()
        {
            var sw = Stopwatch.StartNew();

            var dict = new Dictionary<string, string>();
            using ( var sr = new StreamReader( GetStream() ) )
            {
                string line;
                while ( ( line = sr.ReadLine() ) != null )
                {
                    if ( string.IsNullOrEmpty( line ) )
                        continue;

                    if ( line.StartsWith( "#" ) )
                        continue;

                    var parts = line.Split('=');
                    if ( parts.Length != 2 )
                        continue;

                    dict.Add( parts[ 0 ].Trim() , parts[ 1 ].Trim() );
                }

                Dictionary = dict;
            }

            sw.Stop();

            log.WriteLine( $"Parsed KeyValueFile in { sw.Elapsed }" );
            log.WriteLine( $"{ Dictionary.Count } entries" );
        }
    }
}
