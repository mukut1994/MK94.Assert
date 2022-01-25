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
			var fullPath = Path.Combine(rootDirectory, path);

			if (!File.Exists(fullPath))
				return null;

			return File.OpenRead(fullPath);
        }

        public void Delete(string file)
		{
			File.Delete(Path.Combine(rootDirectory, file));
		}

        public void Write(string path, Stream sourceStream)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(rootDirectory, path)));

			using var targetStream = File.Open(Path.Combine(rootDirectory, path), FileMode.Create);
			using var writer = new StreamWriter(targetStream);
			sourceStream.CopyTo(targetStream);
			targetStream.Flush();
		}
    }
}
