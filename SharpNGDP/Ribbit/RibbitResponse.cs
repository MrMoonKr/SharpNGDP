using MimeKit;
using SharpNGDP.TACT.PSV;
using System;
using System.IO;
using System.Text;

namespace SharpNGDP.Ribbit
{
    public class RibbitResponse
    {
        private RibbitResponse(RibbitRequest request)
        {
            Request = request;
            Received = DateTime.Now;
        }

        public RibbitResponse(RibbitRequest request, Stream stream)
            : this(request)
        {
            // Consider if this is a bad idea
            // (instead of parsing Mime directly)
            using (var sr = new StreamReader(stream))
            {
                Message = sr.ReadToEnd();
            }
            // TODO: Throw exception if bad request (no response)
        }

        public DateTime Received { get; }
        public RibbitRequest Request { get; }
        public string Message { get; }

        public Stream CreateStream() => new MemoryStream(Encoding.UTF8.GetBytes(Message), false);

        private MimeMessage _mimeMessage = null;
        public MimeMessage GetMimeMessage() => _mimeMessage ?? (_mimeMessage = MimeMessage.Load(CreateStream()));

        private PSVFile _psvFile = null;
        public PSVFile GetPSVFile() => _psvFile ?? (_psvFile = PSVParser.Parse(GetMimeMessage().TextBody));

        public string SequenceNumber => GetPSVFile().SequenceNumber;
    }
}
