using SharpNGDP.Ribbit;
using SharpNGDP.TACT;
using System.IO;

namespace SharpNGDP.Managers
{
    /// <summary>
    /// 로컬 파일 캐싱 관리자
    /// </summary>
    public class FileManager
    {
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="ngdpClient">웹 클라이언트</param>
        /// <param name="basePath">로컬 저장소 디렉토리</param>
        public FileManager( NGDPClient ngdpClient , string basePath )
        {
            NGDPClient  = ngdpClient;
            BasePath    = basePath;
        }

        private NGDPClient  NGDPClient { get; }
        public string       BasePath { get; }

        public NGDPResponse Get( NGDPRequest request )
        {
            switch ( request )
            {
                case RibbitRequest req:
                    {
                        var res = NGDPClient.RibbitClient.Execute( req );

                        var fp  = Path.Combine( BasePath, "ribbit", res.GetFile<RibbitFile>().Checksum );
                        if ( !File.Exists( fp ) )
                        {
                            WriteStreamToFile( res.GetStream() , fp );
                        }

                        return res;
                    }

                case TACTRequest req:
                    {
                        // 로컬 캐시된 파일 있으면 로컬 파일스트림으로 응답 생성.
                        var fp = Path.Combine( BasePath, "cdn", req.URI.AbsolutePath.TrimStart( '/' ) );
                        if ( File.Exists( fp ) )
                        {
                            using ( var fs = new FileStream( fp , FileMode.Open , FileAccess.Read ) )
                            {
                                return new TACTResponse( req , fs );
                            }
                        }

                        // 원격 다운로드 후 로컬로 저장
                        var res = NGDPClient.TACTClient.Get( req );
                        WriteStreamToFile( res.GetStream() , fp );

                        return res;
                    }

                default:
                    {
                        return null;
                    }
            }
        }

        /// <summary>
        /// 스트림을 로컬 파일로 저장.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="filePath"></param>
        private static void WriteStreamToFile( Stream stream , string filePath )
        {
            Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );
            using ( var fs = new FileStream( filePath , FileMode.Create , FileAccess.Write ) )
            {
                stream.CopyTo( fs );
            }
        }
    }
}
