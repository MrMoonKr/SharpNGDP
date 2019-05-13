using SharpNGDP.Files;
using SharpNGDP.Records;
using SharpNGDP.Ribbit;
using SharpNGDP.Ribbit.PSV;
using SharpNGDP.TACT;
using SharpNGDP.Extensions;
using System;
using System.IO;
using System.Linq;

namespace SharpNGDP
{
    class Program
    {
        static void Main(string[] args)
        {
            //printSummary();
            //printProducts();
            //downloadWoWConfigs();
            //installProduct("wow_classic_beta");

            readLoop();
        }

        private static void readLoop()
        {
            var ngdp = new NGDPClient();
            do
            {
                Console.WriteLine("Enter command:");
                var cmd = Console.ReadLine();
                if (string.IsNullOrEmpty(cmd)) break;

                var response = ngdp.RibbitClient.Execute(cmd);
                var file = response.GetFile<RibbitFile>();
                Console.WriteLine(file.MimeMessage.TextBody);
                Console.WriteLine();
                Console.WriteLine("Press ENTER to exit or any other key to continue");
            } while (Console.ReadKey().Key != ConsoleKey.Enter);
        }

        private static void printSummary()
        {
            var ngdp = new NGDPClient();
            var summary = ngdp.RibbitClient.GetSummary();
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
            var ngdp = new NGDPClient();
            // Only known WoW products
            var productNames = new string[] { "wow", "wow_beta", "wow_classic", "wow_classic_beta", "wowdev", "wowe1", "wowe2", "wowe3", "wowt", "wowv", "wowz" };
            // Filter inactive products
            var summary = ngdp.RibbitClient.Execute("v1/summary").GetFile<PSVFile>().AsRecords<SummaryRecord>();
            var filteredProductNames = productNames.Where(p => summary.Any(s => s.Product == p && string.IsNullOrEmpty(s.Flags)));

            // Format string for alignment
            const string PRODUCT_ALIGN_FORMAT = "{0,-18} | {1, 4} | {2, 4} | {3, 15} | {4, 6}";

            var header = string.Format(PRODUCT_ALIGN_FORMAT, "Name", "#Ver", "#CDN", "Version", "Build");
            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));
            foreach (var productName in filteredProductNames)
            {
                var versions = ngdp.RibbitClient.GetProductVersions(productName);
                var cdns = ngdp.RibbitClient.GetProductCDNs(productName);
                var freshest = versions.OrderByDescending(v => v.BuildId).FirstOrDefault();

                Console.WriteLine(PRODUCT_ALIGN_FORMAT,
                    productName, versions.Count(), cdns.Count(), freshest?.VersionsName ?? "-", freshest?.BuildId ?? "-");
            }
            Console.WriteLine();
        }

        private static void downloadWoWConfigs()
        {
            var ngdp = new NGDPClient();

            var version = ngdp.RibbitClient.GetProductVersions("wow").OrderByDescending(v => v.BuildId).FirstOrDefault();
            var cdn = ngdp.TACTClient.GetPreferredCDN(ngdp.RibbitClient.GetProductCDNs("wow"));

            var buildConfig = ngdp.TACTClient
                .Get(new TACTRequest(cdn, CDNRequestType.Config, version.BuildConfig))
                .GetFile<KeyValueFile>();
            Console.WriteLine($"{version.BuildConfig} BuildConfig");
            foreach (var kvp in buildConfig.Dictionary)
                Console.WriteLine("{0, 30} | {1}", kvp.Key, kvp.Value);
            Console.WriteLine();

            var cdnConfig = ngdp.TACTClient
                .Get(new TACTRequest(cdn, CDNRequestType.Config, version.CDNConfig))
                .GetFile<KeyValueFile>();
            Console.WriteLine($"{version.CDNConfig} CDNConfig");
            foreach (var kvp in cdnConfig.Dictionary)
                Console.WriteLine("{0, 30} | {1}", kvp.Key, kvp.Value);
            Console.WriteLine();

            foreach (var archive in cdnConfig.Dictionary["archives"].Split(' '))
            {
                Console.WriteLine($"{archive}.index ArchiveIndexFile");
                var archiveIndex = ngdp.TACTClient
                    .Get(new TACTRequest(cdn, CDNRequestType.Data, $"{archive}.index"))
                    .GetFile<ArchiveIndexFile>();
            }

            Console.WriteLine($"{buildConfig.Dictionary["install"].Split(' ')[1]} InstallFile");
            var installFile = ngdp.TACTClient
                .Get(new TACTRequest(cdn, CDNRequestType.Data, buildConfig.Dictionary["install"].Split(' ')[1]))
                .GetFile<InstallFile>();

            Console.WriteLine($"{buildConfig.Dictionary["download"].Split(' ')[1]} DownloadFile");
            var downloadFile = ngdp.TACTClient
                .Get(new TACTRequest(cdn, CDNRequestType.Data, buildConfig.Dictionary["download"].Split(' ')[1]))
                .GetFile<DownloadFile>();

            // This file is yuge
            Console.WriteLine($"{buildConfig.Dictionary["encoding"].Split(' ')[1]} EncodingFile");
            var encodingFile = ngdp.TACTClient
                .Get(new TACTRequest(cdn, CDNRequestType.Data, buildConfig.Dictionary["encoding"].Split(' ')[1]))
                .GetFile<EncodingFile>();
        }

        private static void CopyToFile(Stream stream, string destpath)
        {
            using (var fs = new FileStream(destpath, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fs);
        }

        private static void installProduct(string productName)
        {
            var ngdp = new NGDPClient();

            Console.WriteLine($"Looking up product '{productName}'");

            var versions = ngdp.RibbitClient.GetProductVersions(productName);
            var version = versions.OrderByDescending(v => v.BuildId).FirstOrDefault();
            Console.WriteLine($"Found {versions.Count()} versions. Selecting build {version.BuildId}.");

            var cdns = ngdp.RibbitClient.GetProductCDNs(productName);
            var cdn = ngdp.TACTClient.GetPreferredCDN(cdns);
            Console.WriteLine($"Found {cdns.Count()} CDNs. Selecting '{cdn.Name}'");

            Console.WriteLine($"Fetching BuildConfig ({version.BuildConfig})");
            var buildConfig = ngdp.TACTClient
                .Get(new TACTRequest(cdn, CDNRequestType.Config, version.BuildConfig))
                .GetFile<KeyValueFile>();

            var encodingFileHash = buildConfig.Dictionary["encoding"].Split(' ')[1];
            Console.WriteLine($"Fetching EncodingFile ({encodingFileHash})");            
            var encodingFile = ngdp.TACTClient
                .Get(new TACTRequest(cdn, CDNRequestType.Data, encodingFileHash))
                .GetFile<EncodingFile>();

            var installFileHash = buildConfig.Dictionary["install"].Split(' ')[1];
            Console.WriteLine($"Fetching InstallFile ({installFileHash})");
            var installFile = ngdp.TACTClient
                .Get(new TACTRequest(cdn, CDNRequestType.Data, installFileHash))
                .GetFile<InstallFile>();

            var tag = installFile.InstallFileTags.FirstOrDefault(t => t.Name.ToLower() == ngdp.Context.Platform.ToLower());
            Console.WriteLine($"InstallFile contains {installFile.InstallFileTags.Length} tags. Selecting '{tag.Name}'");

            var files = installFile.GetEntriesWithTag(tag);
            Console.WriteLine($"{files.Count()} entries tagged. Downloading...");

            var baseDir = version.BuildConfig;
            Directory.CreateDirectory(baseDir);

            foreach (var file in files)
            {
                var encodingEntry = encodingFile.ContentKeyEntries.FirstOrDefault(e => e.CKey.ToHexString() == file.Hash.ToHexString());
                var ekey = encodingEntry.EKey.FirstOrDefault().ToHexString();
                Console.WriteLine($"'{file.Name}' ({file.Size} bytes): CKey {file.Hash.ToHexString()} -> EKey {ekey}");

                Console.WriteLine($"Fetching {ekey}");
                var stream = ngdp.TACTClient.Get(new TACTRequest(cdn, CDNRequestType.Data, ekey)).GetFile<BLTEFile>().GetStream();
                var destPath = Path.Combine(baseDir, file.Name.Replace('/', '\\'));
                
                CopyToFile(stream, destPath);
            }

            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}
