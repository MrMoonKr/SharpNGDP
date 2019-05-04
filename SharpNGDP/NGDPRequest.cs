using System;

namespace SharpNGDP
{
    public abstract class NGDPRequest
    {
        protected NGDPRequest(Uri uri)
        {
            URI = uri;
        }

        protected NGDPRequest(string uri)
            : this(new Uri(uri))
        { }

        public Uri URI { get; }
    }
}