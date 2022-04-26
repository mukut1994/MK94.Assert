using System.IO;
using System.Collections.Concurrent;

namespace MK94.Assert.Output
{
    public class MemoryFileOutput : IFileOutput
    {
		public ConcurrentDictionary<string, string> Files { get; } = new ConcurrentDictionary<string, string>();

        public Stream OpenRead(string path)
        {
            if (!Files.ContainsKey(path))
                return null;

            var ret = new MemoryStream();

            using var writer = new StreamWriter(ret, System.Text.Encoding.UTF8, 1024, true);

            writer.Write(Files[path]);
            writer.Flush();
            ret.Position = 0;

            return ret;
        }

        public void Delete(string file)
        {
			Files.TryRemove(file, out _);
        }

        public void Write(string file, Stream sourceStream)
        {
			using var reader = new StreamReader(sourceStream);
			Files[file] = reader.ReadToEnd();
        }
    }
}
