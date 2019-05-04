using System;
using System.IO;

namespace SharpNGDP.Ribbit
{
    public class RibbitResponse : NGDPResponse
    {
        public RibbitResponse(RibbitRequest request, Stream stream)
            : base(request, stream)
        { }
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
