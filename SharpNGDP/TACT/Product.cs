using SharpNGDP.Ribbit;
using SharpNGDP.TACT.Responses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SharpNGDP.TACT
{
    public class Product
    {
        public Product(string name)
        {
            Name = name;

            Versions = GetVersions().ToList().AsReadOnly();
            CDNs = GetCDNs().ToList().AsReadOnly();
        }

        public string Name { get; }

        public ReadOnlyCollection<ProductVersion> Versions { get; }
        public ReadOnlyCollection<ProductCDN> CDNs { get; }

        public ProductVersion GetVersionByNewestBuild()
        {
            return Versions.OrderByDescending(v => v.BuildId).FirstOrDefault();
        }

        public ProductCDN GetCDNForVersion(ProductVersion version)
        {
            return GetCDNForVersion(version.Region);
        }

        public ProductCDN GetCDNForVersion(string region)
        {
            return CDNs.FirstOrDefault(c => c.Name == region);
        }

        private IEnumerable<ProductVersion> GetVersions()
        {
            var client = new RibbitClient();
            var response = client.Execute($"v1/products/{Name}/versions");
            return response.GetPSVFile().MapTo<ProductVersion>();
        }

        private IEnumerable<ProductCDN> GetCDNs()
        {
            var client = new RibbitClient();
            var response = client.Execute($"v1/products/{Name}/cdns");
            return response.GetPSVFile().MapTo<ProductCDN>();
        }
    }
}
