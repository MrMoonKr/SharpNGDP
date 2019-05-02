using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SharpNGDP.Ribbit
{
    public class RibbitRequest
    {
        public RibbitRequest(Uri uri)
        {
            URI = uri;
        }
        
        public Uri URI { get; }

        public string Host => URI.Host;
        public int Port => URI.Port;
        public string Command => URI.AbsolutePath.TrimStart('/');
    }
}
