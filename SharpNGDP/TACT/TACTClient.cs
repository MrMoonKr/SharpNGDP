using SharpNGDP.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SharpNGDP.TACT
{
    public class TACTClient
    {
        private static Logger log = Logger.Create<TACTClient>();

        private static readonly HttpClient HttpClient = new HttpClient();

        public TACTClient(NGDPContext context)
        {
            Context = context;
        }

        public NGDPContext Context { get; }

        public TACTResponse Get(TACTRequest request)
        {
            log.WriteLine($"Executing request for {request.URI}");
            return new TACTResponse(request, HttpClient.GetStreamAsync(request.URI).Result);
        }

        public TACTResponse Get(Uri uri) =>
            Get(new TACTRequest(uri));

        public TACTResponse Get(string url) =>
            Get(new TACTRequest(url));


        public CDNRecord GetPreferredCDN(IEnumerable<CDNRecord> cdns) =>
            cdns.Where(cdn => Context.PreferredCDNs.Contains(cdn.Name))
            .OrderBy(cdn => Array.IndexOf(Context.PreferredCDNs, cdn.Name))
            .FirstOrDefault();

    }
}
