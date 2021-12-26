using SharpNGDP.Records;
using System;

namespace SharpNGDP.TACT
{
    public enum CDNRequestType
    {
        Config,
        Data,
        Patch,
        Product,
    };

    public class TACTRequest : NGDPRequest
    {
        public TACTRequest( Uri uri )
            : base( uri )
        { }

        public TACTRequest( string url )
            : base( url )
        { }

        public TACTRequest( CDNRecord cdn , CDNRequestType requestType , string fileHash )
            : base( $"http://{ cdn.Hosts[ 0 ] }/{ GetPath( cdn , requestType ) }/{ CreatePartitionedHash( fileHash ) }" )
        { }

        /// <summary>
        /// HEX:16 으로부터 2/2/16 형태의 경로 문자열 생성.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private static string CreatePartitionedHash( string hash )
            => $"{ hash.Substring( 0 , 2 ) }/{ hash.Substring( 2 , 2 ) }/{ hash }";

        private static string GetPath( CDNRecord cdn , CDNRequestType requestType )
        {
            switch ( requestType )
            {
                case CDNRequestType.Product:
                    return cdn.ConfigPath;
                default:
                    return $"{ cdn.Path }/{ requestType.ToString().ToLower() }";
            }
        }
    }
}
