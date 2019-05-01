using MimeKit;
using SharpNGDP.TACT.PSV;
using System;
using System.Collections.Generic;
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
        }

        public DateTime Received { get; }
        public RibbitRequest Request { get; }

        public string Message { get; }

        public Stream CreateStream() => new MemoryStream(Encoding.UTF8.GetBytes(Message), false);

        public MimeMessage GetMimeMessage() => MimeMessage.Load(CreateStream());

        public PSVFile GetPSVFile() => PSVParser.Parse(GetMimeMessage().TextBody);        
    }
}
