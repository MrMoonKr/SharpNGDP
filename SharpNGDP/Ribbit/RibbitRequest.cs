using System;

namespace SharpNGDP.Ribbit
{
    public class RibbitRequest : NGDPRequest
    {
        public RibbitRequest(Uri uri)
            : base(uri)
        { }

        public RibbitRequest(string uri)
            : base(uri)
        { }

        public RibbitRequest(string host, int port, string command)
            : base($"ribbit://{host}:{port}/{command.TrimStart('/')}")
        { }

        public RibbitRequest(NGDPContext context, string command)
            : this(context.RibbitHost, context.RibbitPort, command.TrimStart('/'))
        { }


        public string Host => URI.Host;
        public int Port => URI.Port;
        public string Command => URI.AbsolutePath.TrimStart('/');
    }
}
