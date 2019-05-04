using System.Collections.Generic;
using System.IO;

namespace SharpNGDP.Files
{
    public class KeyValueFile : NGDPFile
    {
        public KeyValueFile(Stream stream)
            : base(stream)
        { }

        public Dictionary<string, string> Dictionary { get; private set; }

        public override void Read()
        {
            var dict = new Dictionary<string, string>();
            using (var sr = new StreamReader(GetStream()))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (line.StartsWith("#"))
                        continue;

                    var parts = line.Split('=');
                    if (parts.Length != 2)
                        continue;

                    dict.Add(parts[0].Trim(), parts[1].Trim());
                }
            }
            Dictionary = dict;
        }
    }
}
