using System.Net.Http;

namespace SharpNGDP.TACT
{
    public class TACTClient
    {
        private static Logger log = Logger.Create<TACTClient>();

        private static readonly HttpClient HttpClient = new HttpClient();

        public TACTClient()
        { }

        public TACTResponse Get(TACTRequest request)
        {
            log.WriteLine($"Executing request for {request.URI}");
            return new TACTResponse(request, HttpClient.GetStreamAsync(request.URI).Result);
        }
    }
}
