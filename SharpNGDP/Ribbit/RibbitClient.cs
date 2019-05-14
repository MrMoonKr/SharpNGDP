using System.IO;
using System.Net.Sockets;

namespace SharpNGDP.Ribbit
{
    public class RibbitClient
    {
        private static Logger log = Logger.Create<RibbitClient>();

        public RibbitClient()
        { }

        public RibbitResponse Execute(RibbitRequest request)
        {
            log.WriteLine($"Executing request for {request.URI}");

            using (var client = new TcpClient(request.Host, request.Port))
            {
                var ns = client.GetStream();

                using (var sw = new StreamWriter(ns))
                {
                    sw.WriteLine(request.Command);
                    sw.Flush();

                    return new RibbitResponse(request, ns);
                }
            }
        }
    }
}
