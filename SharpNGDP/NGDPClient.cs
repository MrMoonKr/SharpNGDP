using SharpNGDP.Managers;
using SharpNGDP.Records;
using SharpNGDP.Ribbit;
using SharpNGDP.Ribbit.PSV;
using SharpNGDP.TACT;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpNGDP
{
    public class NGDPClient
    {
        public NGDPClient(NGDPContext context)
        {
            Context = context;

            RibbitClient = new RibbitClient();
            TACTClient = new TACTClient();

            FileManager = new FileManager(this, Context.LocalCache);
        }

        public NGDPClient()
            : this(NGDPContext.DefaultContext)
        { }

        public NGDPContext Context { get; private set; }

        public RibbitClient RibbitClient { get; private set; }
        public TACTClient TACTClient { get; private set; }

        public FileManager FileManager { get; private set; }

        public IEnumerable<SummaryRecord> GetSummary()
        {
            var response = FileManager.Get(new RibbitRequest(Context, $"v1/summary"));
            return response.GetFile<PSVFile>().AsRecords<SummaryRecord>();
        }

        public IEnumerable<VersionRecord> GetProductVersions(string productName)
        {
            var response = FileManager.Get(new RibbitRequest(Context, $"v1/products/{productName}/versions"));
            return response.GetFile<PSVFile>().AsRecords<VersionRecord>();
        }

        public IEnumerable<CDNRecord> GetProductCDNs(string productName)
        {
            var response = FileManager.Get(new RibbitRequest(Context, $"v1/products/{productName}/cdns"));
            return response.GetFile<PSVFile>().AsRecords<CDNRecord>();
        }

        public CDNRecord GetPreferredCDN(IEnumerable<CDNRecord> cdns) =>
            cdns.Where(cdn => Context.PreferredCDNs.Contains(cdn.Name))
            .OrderBy(cdn => Array.IndexOf(Context.PreferredCDNs, cdn.Name))
            .FirstOrDefault();
    }
}
