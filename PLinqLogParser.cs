using System;
using System.IO;
using System.Linq;

namespace DataParallelismTask.LogParsers
{
    public class PLinqLogParser : ILogParser
    {
        private readonly FileInfo _file;
        private readonly Func<string, string?> _tryGetIdFromLine;

        public PLinqLogParser(FileInfo file, Func<string, string?> tryGetIdFromLine)
        {
            _file = file;
            _tryGetIdFromLine = tryGetIdFromLine;
        }

        public string[] GetRequestedIdsFromLogFile() => 
            File.ReadLines(_file.FullName)
                .AsParallel()
                .Select(_tryGetIdFromLine)
                .Where(id => id != null)
                .ToArray();
    }
}
