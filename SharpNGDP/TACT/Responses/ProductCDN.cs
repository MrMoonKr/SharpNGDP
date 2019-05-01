using SharpNGDP.TACT.PSV;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpNGDP.TACT.Responses
{
    public class ProductCDN : PSVResponse
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string[] Hosts { get; set; }
        public string[] Servers { get; set; }
        public string ConfigPath { get; set; }

        public string PathURL => $"http://{Hosts[0]}/{Path}/";
        public string ConfigPathURL => $"http://{Hosts[0]}/{ConfigPath}/";

        public override bool Map(string key, string value)
        {
            switch (key)
            {
                case "Name": Name = value; break;
                case "Path": Path = value; break;
                case "Hosts": Hosts = value.Split(' '); break;
                case "Servers": Servers = value.Split(' '); break;
                case "ConfigPath": ConfigPath = value; break;
                default:
                    return false;
            }
            return true;
        }
    }
}
