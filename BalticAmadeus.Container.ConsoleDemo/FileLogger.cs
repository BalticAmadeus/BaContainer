using System;
using System.IO;

namespace BalticAmadeus.Container.ConsoleDemo
{
    public class FileLogger : IDisposable
    {
        private readonly StreamWriter _streamWriter;

        public FileLogger(string path)
        {
            _streamWriter = new StreamWriter(path);
        }

        public void Log(string text)
        {
            _streamWriter.WriteLine(text);
            _streamWriter.Flush();
        }

        public void Dispose()
        {
            _streamWriter.Dispose();
        }
    }
}