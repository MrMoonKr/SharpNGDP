using SharpNGDP.TACT.PSV;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpNGDP.TACT.Responses
{
    public class Summary : PSVResponse
    {
        public string Product { get; set; }
        public string Seqn { get; set; }
        public string Flags { get; set; }

        public override bool Map(string key, string value)
        {
            switch (key)
            {
                case "Product": Product = value; break;
                case "Seqn": Seqn = value; break;
                case "Flags": Flags = value; break;
                default: return false;
            }
            return true;
        }
    }
}
