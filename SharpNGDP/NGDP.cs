using SharpNGDP.Ribbit;
using SharpNGDP.TACT.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharpNGDP
{
    public class NGDP
    {
        public NGDP(NGDPContext context)
        {
            Context = context;
        }

        public NGDP()
            : this(NGDPContext.DefaultContext)
        { }

        private static readonly HttpClient HttpClient = new HttpClient();

        public NGDPContext Context { get; }


        private string CreatePartitionedHash(string hash)
            => $"{hash.Substring(0, 2)}/{hash.Substring(2, 2)}/{hash}";


        private Task<HttpResponseMessage> GetFileAsync(string url)
            => HttpClient.GetAsync(url);

        public Task<HttpResponseMessage> GetFileAsync(string cdnHost, string cdnPath, string pathType, string fullHash)
            => GetFileAsync($"http://{cdnHost}/{cdnPath}/{pathType}/{CreatePartitionedHash(fullHash)}");

        public Task<HttpResponseMessage> GetFileAsync(ProductCDN cdn, string pathType, string fullHash)
            => GetFileAsync(cdn.Hosts[0], cdn.Path, pathType, fullHash);


        public Task<HttpResponseMessage> GetConfigFileAsync(ProductCDN cdn, string hash)
            => GetFileAsync(cdn, "config", hash);

        public Task<HttpResponseMessage> GetDataFileAsync(ProductCDN cdn, string hash)
            => GetFileAsync(cdn, "data", hash);

        public Task<HttpResponseMessage> GetPatchFileAsync(ProductCDN cdn, string hash)
            => GetFileAsync(cdn, "patch", hash);


        public Task<HttpResponseMessage> GetProductConfigFileAsync(ProductCDN cdn, string hash)
            => GetFileAsync($"http://{cdn.Hosts[0]}/{cdn.ConfigPath}/{CreatePartitionedHash(hash)}");

        public Task<HttpResponseMessage> GetProductConfigFileAsync(ProductCDN cdn, ProductVersion version)
            => GetFileAsync($"http://{cdn.Hosts[0]}/{cdn.ConfigPath}/{CreatePartitionedHash(version.ProductConfig)}");


        public IEnumerable<Summary> GetSummary()
        {
            var client = new RibbitClient(Context);
            var response = client.Execute($"v1/summary");
            return response.GetPSVFile().MapTo<Summary>();
        }

        public IEnumerable<ProductVersion> GetProductVersions(string productName)
        {
            var client = new RibbitClient(Context);
            var response = client.Execute($"v1/products/{productName}/versions");
            return response.GetPSVFile().MapTo<ProductVersion>();
        }

        public IEnumerable<ProductCDN> GetProductCDNs(string productName)
        {
            var client = new RibbitClient(Context);
            var response = client.Execute($"v1/products/{productName}/cdns");
            return response.GetPSVFile().MapTo<ProductCDN>();
        }

        public ProductCDN GetPrefferedCDN(IEnumerable<ProductCDN> cdns) =>
            cdns.Where(cdn => Context.PreferredCDNs.Contains(cdn.Name))
            .OrderBy(cdn => Array.IndexOf(Context.PreferredCDNs, cdn.Name))
            .FirstOrDefault();
    }
}
