using System.IO;

namespace SharpNGDP.TACT
{
    public class TACTResponse : NGDPResponse
    {
        public TACTResponse(TACTRequest request, Stream stream)
            : base(request, stream)
        { }
    }
}
