using System.Collections.Generic;
using System.IO;
using System.Collections.Concurrent;

namespace MK94.Assert.Output
{
    public class MemoryFileOutput : IFileOutput
    {
		private Dictionary<string, string> rootFile = new Dictionary<string, string>();
		private ConcurrentDictionary<string, string> files = new ConcurrentDictionary<string, string>();

        public Stream OpenRead(string path)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string file)
        {
			files.TryRemove(file, out _);
        }

        public Dictionary<string, string> LoadRootFile()
        {
			return rootFile;
        }

        public void Write(string file, Stream sourceStream)
        {
			using var reader = new StreamReader(sourceStream);
			files[file] = reader.ReadToEnd();
        }
    }
}
