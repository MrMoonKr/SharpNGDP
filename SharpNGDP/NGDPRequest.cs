using System;


namespace SharpNGDP
{
    /// <summary>
    /// 클라이언트 요청 추상화.
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
        {
            // nothing
        }

        /// <summary>
        /// API 요청 주소
        /// </summary>
        public Uri URI { get; }
    }
}
