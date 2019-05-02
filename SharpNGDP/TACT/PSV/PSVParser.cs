using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpNGDP.TACT.PSV
{
    public class PSVParser
    {
        private static Regex s_SequenceNumberRegex = new Regex(@"seqn = (\d+)", RegexOptions.Compiled);

        public static PSVFile Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new PSVParseException("No input");

            using (var sr = new StringReader(text))
            {
                // Read header
                var header = sr.ReadLine().Split('|');
                for (var c = 0; c < header.Length; c++)
                    header[c] = header[c].Substring(0, header[c].IndexOf('!'));

                var seqn = string.Empty;

                var rows = new List<string[]>();
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();

                    // Ignore empty lines
                    if (string.IsNullOrEmpty(line))
                        continue;

                    // Ignore comments
                    if (line.StartsWith("#"))
                    {
                        // ... unless it's the sequence number :)
                        var seqnMatch = s_SequenceNumberRegex.Match(line);
                        if (seqnMatch.Success)
                            seqn = seqnMatch.Groups[1].Value;

                        continue;
                    }

                    var row = line.Split('|');
                    if (header.Length != row.Length)
                        throw new PSVParseException($"Column number mismatch between header ({header.Length}) and row ({row.Length}) at row {line}");
                    rows.Add(row);
                }

                return new PSVFile(seqn, header, rows.ToArray());
            }
        }
    }

    public class PSVParseException : Exception
    {
        public PSVParseException()
        { }

        public PSVParseException(string message)
            : base(message)
        { }
    }
}
