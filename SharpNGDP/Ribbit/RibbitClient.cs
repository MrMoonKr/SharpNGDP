using SharpNGDP.Records;
using SharpNGDP.Ribbit.PSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace SharpNGDP.Ribbit
{
    public class RibbitClient
    {
        private static Logger log = Logger.Create<RibbitClient>();

        public RibbitClient(NGDPContext context)
        { 
            Host = context.RibbitHost;
            Port = context.RibbitPort;

            log.WriteLine($"Using ribbit://{Host}:{Port}/ as base for implicit ribbit requests");
        }

        public string Host { get; private set; }
        public int Port { get; private set; }

        public RibbitResponse Execute(RibbitRequest request)
        {
            log.WriteLine($"Executing request for {request.URI}");

            using (var client = new TcpClient(request.Host, request.Port))
            {
                var ns = client.GetStream();

                using (var sw = new StreamWriter(ns))
                {
                    sw.WriteLine(request.Command);
                    sw.Flush();

                    return new RibbitResponse(request, ns);
                }
            }
        }

        public RibbitResponse Execute(string path) =>
            Execute(CreateRibbitURIWithContext(path));

        public RibbitResponse Execute(Uri uri) =>
            Execute(new RibbitRequest(uri));


        public IEnumerable<SummaryRecord> GetSummary()
        {
            var response = Execute($"v1/summary");
            return response.GetFile<PSVFile>().AsRecords<SummaryRecord>();
        }

        public IEnumerable<VersionRecord> GetProductVersions(string productName)
        {
            var response = Execute($"v1/products/{productName}/versions");
            return response.GetFile<PSVFile>().AsRecords<VersionRecord>();
        }

        public IEnumerable<CDNRecord> GetProductCDNs(string productName)
        {
            var response = Execute($"v1/products/{productName}/cdns");
            return response.GetFile<PSVFile>().AsRecords<CDNRecord>();
        }


        private Uri CreateRibbitURIWithContext(string path)
        {
            var uri = new Uri(path, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
                return uri;
            return new Uri($"ribbit://{Host}:{Port}/{path.TrimStart('/')}");
        }
    }
}
