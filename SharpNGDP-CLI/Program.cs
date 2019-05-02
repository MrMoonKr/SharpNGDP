using SharpNGDP;
using SharpNGDP.Ribbit;
using SharpNGDP.TACT;
using SharpNGDP.TACT.Responses;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SharpTact
{
    class Program
    {
        static void Main(string[] args)
        {
            printSummary();
            printProducts();
            downloadWoWConfigs();

            readLoop();
        }

        private static void readLoop()
        {
            var ngdp = new NGDP();
            var client = new RibbitClient(ngdp.Context);
            do
            {
                Console.WriteLine("Enter command:");
                var cmd = Console.ReadLine();
                if (string.IsNullOrEmpty(cmd)) break;

                var response = client.Execute(cmd);
                Console.WriteLine(response.Message);
                Console.WriteLine();
                Console.WriteLine("Press ENTER to exit or any other key to continue");
            } while (Console.ReadKey().Key != ConsoleKey.Enter);
        }

        private static void printSummary()
        {
            var ngdp = new NGDP();
            var summary = new RibbitClient(ngdp.Context).Execute("v1/summary").GetPSVFile().MapTo<Summary>();
            const string SUMMARY_ALIGN_FORMAT = "{0,-20} | {1, 8} | {2, -8}";
            var header = string.Format(SUMMARY_ALIGN_FORMAT, "Product", "Seq #", "Flags");
            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));
            foreach (var s in summary)
            {
                Console.WriteLine(SUMMARY_ALIGN_FORMAT,
                    s.Product, s.Seqn, s.Flags);
            }
            Console.WriteLine();
        }

        private static void printProducts()
        {
            var ngdp = new NGDP();
            // Only known WoW products
            var productNames = new string[] { "wow", "wow_beta", "wow_classic", "wow_classic_beta", "wowdev", "wowe1", "wowe2", "wowe3", "wowt", "wowv", "wowz" };
            // Filter inactive products
            var summary = new RibbitClient(ngdp.Context).Execute("v1/summary").GetPSVFile().MapTo<Summary>();
            var filteredProductNames = productNames.Where(p => summary.Any(s => s.Product == p && string.IsNullOrEmpty(s.Flags)));

            // Format string for alignment
            const string PRODUCT_ALIGN_FORMAT = "{0,-18} | {1, 4} | {2, 4} | {3, 15} | {4, 6}";

            var header = string.Format(PRODUCT_ALIGN_FORMAT, "Name", "#Ver", "#CDN", "Version", "Build");
            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));
            foreach (var productName in filteredProductNames)
            {
                var versions = ngdp.GetProductVersions(productName);
                var cdns = ngdp.GetProductCDNs(productName);
                var freshest = versions.OrderByDescending(v => v.BuildId).FirstOrDefault();

                Console.WriteLine(PRODUCT_ALIGN_FORMAT,
                    productName, versions.Count(), cdns.Count(), freshest?.VersionsName ?? "-", freshest?.BuildId ?? "-");
            }
            Console.WriteLine();
        }

        private static void downloadWoWConfigs()
        {
            var ngdp = new NGDP();

            var version = ngdp.GetProductVersions("wow").OrderByDescending(v => v.BuildId).FirstOrDefault();
            var cdn = ngdp.GetPrefferedCDN(ngdp.GetProductCDNs("wow"));

            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");

            var response = ngdp.GetConfigFileAsync(cdn, version.BuildConfig).Result;
            var stream = response.Content.ReadAsStreamAsync().Result;
            CopyToFile(stream, $"data/{version.BuildConfig}");
        }

        private static void CopyToFile(Stream stream, string destpath)
        {
            using (var fs = new FileStream(destpath, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fs);
        }
    }
}
