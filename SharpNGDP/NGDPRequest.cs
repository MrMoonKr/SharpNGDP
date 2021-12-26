using System;

namespace SharpNGDP
{
    /// <summary>
    /// 웹 요청 추상화.
    /// 요청 URI 랩퍼.
    /// </summary>
    public abstract class NGDPRequest
    {
        protected NGDPRequest( Uri uri )
        {
            URI = uri;
        }

        protected NGDPRequest( string uri )
            : this( new Uri( uri ) )
        { }

        public Uri URI { get; }
    }
}