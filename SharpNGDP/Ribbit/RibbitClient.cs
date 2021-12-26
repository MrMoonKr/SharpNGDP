using System.IO;
using System.Net.Sockets;


namespace SharpNGDP.Ribbit
{
    /// <summary>
    /// TCP Socket 을 통한 데이터 요청 클라이언트.
    /// </summary>
    public class RibbitClient
    {
        private static Logger log = Logger.Create<RibbitClient>();

        public RibbitClient()
        { }

        /// <summary>
        /// 요청객체를 통해 반환되는 데이터를 응답객체로 반환
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public RibbitResponse Execute( RibbitRequest request )
        {
            log.WriteLine( $"Executing request for {request.URI}" );

            using ( var client = new TcpClient( request.Host , request.Port ) )
            {
                var ns = client.GetStream();

                using ( var sw = new StreamWriter( ns ) )
                {
                    sw.WriteLine( request.Command );
                    sw.Flush();

                    return new RibbitResponse( request , ns );
                }
            }
        }
    }
}
