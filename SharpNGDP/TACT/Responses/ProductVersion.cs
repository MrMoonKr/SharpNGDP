using SharpNGDP.TACT.PSV;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpNGDP.TACT.Responses
{
    public class ProductVersion : PSVResponse
    {
        public string Region { get; set; }
        public string BuildConfig { get; set; }
        public string CDNConfig { get; set; }
        public string KeyRing { get; set; }
        public string BuildId { get; set; }
        public string VersionsName { get; set; }
        public string ProductConfig { get; set; }

        public override bool Map(string key, string value)
        {
            switch (key)
            {
                case "Region": Region = value; break;
                case "BuildConfig": BuildConfig = value; break;
                case "CDNConfig": CDNConfig = value; break;
                case "KeyRing": KeyRing = value; break;
                case "BuildId": BuildId = value; break;
                case "VersionsName": VersionsName = value; break;
                case "ProductConfig": ProductConfig = value; break;
                default: return false;
            }
            return true;
        }
    }
}
