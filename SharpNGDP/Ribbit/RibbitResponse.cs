using MimeKit;
using SharpNGDP.TACT.PSV;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpNGDP.Ribbit
{
    public class RibbitResponse
    {
        private Regex s_ChecksumRegex = new Regex(@"Checksum: (\w+)", RegexOptions.Compiled);

        private RibbitResponse(RibbitRequest request)
        {
            Request = request;
            Received = DateTime.UtcNow;
        }

        public RibbitResponse(RibbitRequest request, Stream stream)
            : this(request)
        {
            using (var sr = new StreamReader(stream))
            {
                if (sr.EndOfStream)
                    throw new EmptyRibbitResponseException("Invalid response from server. Likely caused by malformed request.");
                Message = sr.ReadToEnd();
            }

            var checksumMatch = s_ChecksumRegex.Match(((MultipartAlternative)GetMimeMessage().Body).Epilogue);
            if (!checksumMatch.Success)
                throw new MalformedRibbitResponseException("Response did not contain checksum in epilogue");
            Checksum = checksumMatch.Groups[1].Value;

            if (string.IsNullOrEmpty(GetPSVFile().SequenceNumber))
                throw new MalformedRibbitResponseException("Response did not contain sequence number");
            SequenceNumber = GetPSVFile().SequenceNumber;
        }

        public DateTime Received { get; }
        public RibbitRequest Request { get; }
        public string Message { get; }
        public string Checksum { get; }
        public string SequenceNumber { get; }

        public Stream CreateStream() => new MemoryStream(Encoding.UTF8.GetBytes(Message), false);

        private MimeMessage _mimeMessage = null;
        public MimeMessage GetMimeMessage() => _mimeMessage ?? (_mimeMessage = MimeMessage.Load(CreateStream()));

        private PSVFile _psvFile = null;
        public PSVFile GetPSVFile() => _psvFile ?? (_psvFile = PSVParser.Parse(GetMimeMessage().TextBody));
    }

    public class RibbitResponseException : Exception
    {
        public RibbitResponseException()
        { }

        public RibbitResponseException(string message)
            : base(message)
        { }
    }

    public class EmptyRibbitResponseException : RibbitResponseException
    {
        public EmptyRibbitResponseException()
        { }

        public EmptyRibbitResponseException(string message)
            : base(message)
        { }
    }

    public class MalformedRibbitResponseException : RibbitResponseException
    {
        public MalformedRibbitResponseException()
        { }

        public MalformedRibbitResponseException(string message)
            : base(message)
        { }
    }
}
