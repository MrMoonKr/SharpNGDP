using MimeKit;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SharpNGDP.Ribbit
{
    public class RibbitClient
    {
        private const string DEFAULT_HOST = "us.version.battle.net";
        private const int DEFAULT_PORT = 1119;

        public RibbitResponse Execute(string path)
        {
            return Execute(new RibbitRequest(path));
        }

        public RibbitResponse Execute(RibbitRequest request)
        {
            var host = request.IsAbsolute ?
                request.Host : DEFAULT_HOST;

            var port = request.IsAbsolute ?
                request.Port : DEFAULT_PORT;

            using (var client = new TcpClient(host, port))
            {
                var stream = client.GetStream();

                // Send command
                var bytes = Encoding.UTF8.GetBytes(request.Command + "\r\n");
                stream.Write(bytes, 0, bytes.Length);

                // Read response
                return new RibbitResponse(request, stream);
            }
        }
    }
}
