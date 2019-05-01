using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SharpNGDP.Ribbit
{
    public class RibbitRequest
    {
        public RibbitRequest(string path)
        {
            var uri = new Uri(path, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                Host = uri.Host;
                Port = uri.Port;
                Command = uri.LocalPath;
            }
            else
            {
                Command = uri.OriginalString;
            }
        }

        public bool IsAbsolute { get; private set; }

        public string Host { get; private set; }
        public int Port { get; private set; }

        public string Command { get; private set; }        
    }
}
