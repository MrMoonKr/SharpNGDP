using SharpNGDP.Ribbit;
using SharpNGDP.TACT;
using SharpNGDP.TACT.Responses;
using System;
using System.Linq;

namespace SharpTact
{
    class Program
    {
        static void Main(string[] args)
        {
            printSummary();
            printProducts();
            readLoop();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void readLoop()
        {
            var client = new RibbitClient();
            do
            {
                Console.WriteLine("Enter command:");
                var cmd = Console.ReadLine();
                if (string.IsNullOrEmpty(cmd)) break;

                var response = client.Execute(cmd);
                Console.WriteLine(response.Message);
                Console.WriteLine();
            } while (Console.ReadKey().Key != ConsoleKey.Enter);
        }

        private static void printSummary()
        {
            var summary = new RibbitClient().Execute("v1/summary").GetPSVFile().MapTo<Summary>();
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
            // Only known WoW products
            var productNames = new string[] { "wow", "wow_beta", "wow_classic", "wow_classic_beta", "wowdev", "wowe1", "wowe2", "wowe3", "wowt", "wowv", "wowz" };
            // Filter inactive products
            var summary = new RibbitClient().Execute("v1/summary").GetPSVFile().MapTo<Summary>();
            var filteredProductNames = productNames.Where(p => summary.Any(s => s.Product == p && string.IsNullOrEmpty(s.Flags)));

            // Format string for alignment
            const string PRODUCT_ALIGN_FORMAT = "{0,-18} | {1, 4} | {2, 4} | {3, 15} | {4, 6}";

            var header = string.Format(PRODUCT_ALIGN_FORMAT, "Name", "#Ver", "#CDN", "Version", "Build");
            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));
            foreach (var productName in filteredProductNames)
            {
                var product = new Product(productName);
                var freshest = product.GetVersionByNewestBuild();
                Console.WriteLine(PRODUCT_ALIGN_FORMAT,
                    product.Name, product.Versions.Count, product.CDNs.Count, freshest?.VersionsName ?? "-", freshest?.BuildId ?? "-");
            }
            Console.WriteLine();
        }
    }
}
