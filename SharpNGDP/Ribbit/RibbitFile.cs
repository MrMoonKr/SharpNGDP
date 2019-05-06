using MimeKit;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpNGDP.Ribbit
{
    public class RibbitFile : NGDPFile
    {
        private static Logger log = Logger.Create<RibbitFile>();

        private Regex s_ChecksumRegex = new Regex(@"Checksum: (\w+)", RegexOptions.Compiled);

        public RibbitFile(Stream stream)
            : base(stream)
        { }

        public string Checksum { get; private set; }

        public MimeMessage MimeMessage { get; private set; }

        public override Stream GetStream() =>
            new MemoryStream(Encoding.UTF8.GetBytes(MimeMessage.TextBody));

        public override void Read()
        {
            var sw = Stopwatch.StartNew();

            MimeMessage = MimeMessage.Load(base.GetStream());
            // throw new EmptyRibbitResponseException("Invalid response from server. Likely caused by malformed request.");
            var checksumMatch = s_ChecksumRegex.Match(((MultipartAlternative)MimeMessage.Body).Epilogue);
            if (!checksumMatch.Success)
                throw new MalformedRibbitResponseException("Response did not contain checksum in epilogue");
            Checksum = checksumMatch.Groups[1].Value;

            sw.Stop();
            log.WriteLine($"Parsed RibbitFile in {sw.Elapsed}");
            log.WriteLine($"Checksum: {Checksum}");
        }
    }
}
