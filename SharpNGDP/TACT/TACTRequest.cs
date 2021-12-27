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

    /// <summary>
    /// HTTP 요청
    /// </summary>
    public class TACTRequest : NGDPRequest
    {
        public TACTRequest( Uri uri )
            : base( uri )
        { }

        public TACTRequest( string url )
            : base( url )
        { }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="cdn">서버</param>
        /// <param name="requestType">요청타입 ( config, data, patch, product )</param>
        /// <param name="fileHash">요청하는 파일 ( HEX:16 )</param>
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
