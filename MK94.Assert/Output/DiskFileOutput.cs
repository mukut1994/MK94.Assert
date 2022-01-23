using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MK94.Assert.Output
{
    public class DiskFileOutput : IFileOutput
	{
		private readonly string rootDirectory;

		public DiskFileOutput(string rootDirectory)
		{
			this.rootDirectory = rootDirectory;

            Directory.CreateDirectory(this.rootDirectory);
		}

        public Stream OpenRead(string path)
        {
			if (!File.Exists(path))
				return null;

			return File.OpenRead(path);
        }

        public void Delete(string file)
		{
			File.Delete(Path.Combine(rootDirectory, file));
		}

        public void Write(string path, Stream sourceStream)
		{
			using var targetStream = File.OpenWrite(Path.Combine(rootDirectory, path));
			using var writer = new StreamWriter(targetStream);
			sourceStream.CopyTo(targetStream);
			targetStream.Flush();
		}
    }
}
