using System.IO;

namespace SharpNGDP.TACT
{
    /// <summary>
    /// 요청에 대한 응답. 
    /// 컨텐츠에 대한 스트림으로 부터 Buffer에 복사 저장
    /// </summary>
    public class TACTResponse : NGDPResponse
    {
        public TACTResponse( TACTRequest request, Stream stream )
            : base( request, stream )
        {
            // nothing
        }
    }
}
