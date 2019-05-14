using System;
using System.Collections.Generic;
using System.IO;

namespace SharpNGDP
{
    public class Logger : ILogger
    {
        private const string LOG_FORMAT = "{0} [{1}]: {2}";

        public static List<ILogger> Consumers { get; } = new List<ILogger>()
        {
            FileLogger.Create(string.Format("logs/log-{0:yyyy-MM-dd_hh-mm-ss}.txt", DateTime.Now)),
            new ConsoleLogger()
        };

        public Logger(string name)
        {
            Name = name;
        }

        public string Name { get; }

        private string FormatMessage(string message, params object[] args) =>
            string.Format(LOG_FORMAT, DateTime.Now, Name, string.Format(message, args));

        public void Write(string content)
        {
            foreach (var log in Consumers)
                log.Write(FormatMessage(content));
        }

        public void WriteLine(string line)
        {
            foreach (var log in Consumers)
                log.WriteLine(FormatMessage(line));
        }

        public void WriteLine(string format, params object[] args)
        {
            foreach (var log in Consumers)
                log.WriteLine(FormatMessage(format, args));
        }

        public static Logger Create<T>() where T : class
        {
            return new Logger(typeof(T).Name);
        }
    }

    public class ConsoleLogger : ILogger
    {
        public void Write(string content)
        {
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write(content);

            Console.ForegroundColor = fg;
        }

        public void WriteLine(string line)
        {
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.WriteLine(line);

            Console.ForegroundColor = fg;
        }
    }

    public class FileLogger : StreamLogger
    {
        public FileLogger(string filename)
            : base(new FileStream(filename, FileMode.Create, FileAccess.Write))
        { }

        public static FileLogger Create(string filename)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            return new FileLogger(filename);
        }
    }

    public abstract class StreamLogger : ILogger
    {
        public StreamLogger(Stream stream)
        {
            streamWriter = new StreamWriter(stream) { AutoFlush = true };
        }

        private StreamWriter streamWriter;

        public void Write(string content)
        {
            streamWriter.Write(content);
        }

        public void WriteLine(string line)
        {
            streamWriter.WriteLine(line);
        }
    }

    public interface ILogger
    {
        void Write(string content);
        void WriteLine(string line);
    }
}
