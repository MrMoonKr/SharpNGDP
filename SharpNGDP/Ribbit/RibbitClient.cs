using System;
using System.Net.Sockets;
using System.Text;

namespace SharpNGDP.Ribbit
{
    public class RibbitClient
    {
        public RibbitClient(NGDPContext context)
        {
            UseContext(context);
        }

        public void UseContext(NGDPContext context)
        {
            Host = context.RibbitHost;
            Port = context.RibbitPort;
        }

        public string Host { get; private set; }
        public int Port { get; private set; }

        private Uri CreateRibbitURI(string path)
        {
            var uri = new Uri(path, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
                return uri;
            return new Uri($"ribbit://{Host}:{Port}/{path.TrimStart('/')}");
        }

        public RibbitResponse Execute(string path)
        {
            return Execute(CreateRibbitURI(path));
        }

        public RibbitResponse Execute(Uri uri)
        {
            return Execute(new RibbitRequest(uri));
        }

        public RibbitResponse Execute(RibbitRequest request)
        {
            using (var client = new TcpClient(request.Host, request.Port))
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
