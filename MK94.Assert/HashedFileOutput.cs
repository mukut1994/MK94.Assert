using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System.Text.Json;
using System.Collections.Concurrent;

namespace MK94.Assert
{
	public abstract class HashedFileOutput
	{
		private struct FileInfo
		{
			public string name;
			public SHA256Managed hashAlgo;
			public MemoryStream stream;

            public FileInfo(string name, SHA256Managed hashAlgo, MemoryStream stream)
            {
                this.name = name;
                this.hashAlgo = hashAlgo;
                this.stream = stream;
            }
        }

        private Dictionary<string, FileInfo> files = new Dictionary<string, FileInfo>();

		public Stream Open(string path)
		{
			var ms = new MemoryStream();
			var hash = new SHA256Managed();
			var cs = new CryptoStream(ms, hash, CryptoStreamMode.Write, true);

			files.Add(path, new FileInfo(path, hash, ms));

			return cs;
		}

		public void SaveAll()
		{
			var oldHashes = new HashSet<string>();
			var hashToFile = new Dictionary<string, string>();
			var filesToWriteByHash = new Dictionary<string, FileInfo>();
			var existingRootFile = LoadRootFile();
			var newRootFile = new Dictionary<string, string>();

			foreach (var file in files)
			{
				var hash = Convert.ToBase64String(file.Value.hashAlgo.Hash!).Replace('/', '-').ToLower();
				existingRootFile.TryGetValue(file.Key, out var oldHash);

				newRootFile[file.Key] = hash;
				hashToFile[hash] = file.Key;

				if (oldHash != null)
					oldHashes.Add(oldHash);

				if ((oldHash == null || !oldHash.Equals(hash)) && !filesToWriteByHash.ContainsKey(hash))
					filesToWriteByHash.Add(hash, file.Value);
			}

			var fileDeleted = false;

			foreach (var file in existingRootFile)
			{
				if (hashToFile.ContainsKey(file.Value))
					continue;

				Delete(file.Key, existingRootFile[file.Key]);
				fileDeleted = true;
			}

			if (fileDeleted || filesToWriteByHash.Any())
				WriteRootFile(newRootFile);

			if (!filesToWriteByHash.Any())
				return;

			foreach (var file in filesToWriteByHash)
			{
				if (oldHashes.Contains(file.Key))
					continue;

				var stream = file.Value.stream;
				stream.Position = 0;
				WriteFile(file.Value.name, file.Key, existingRootFile.GetValueOrDefault(file.Value.name), stream);
			}
		}

		protected abstract Dictionary<string, string> LoadRootFile();
		protected abstract void WriteRootFile(Dictionary<string, string> root);
		protected abstract void WriteFile(string file, string hash, string? oldHash, Stream sourceStream);
		protected abstract void Delete(string file, string hash);
	}

    public class MemoryFileOutput : HashedFileOutput
    {
		private Dictionary<string, string> rootFile = new Dictionary<string, string>();
		private ConcurrentDictionary<string, string> files = new ConcurrentDictionary<string, string>();

        protected override void Delete(string file, string hash)
        {
			files.TryRemove(hash, out _);
        }

        protected override Dictionary<string, string> LoadRootFile()
        {
			return rootFile;
        }

        protected override void WriteFile(string file, string hash, string oldHash, Stream sourceStream)
        {
			using var reader = new StreamReader(sourceStream);
			files[hash] = reader.ReadToEnd();
        }

        protected override void WriteRootFile(Dictionary<string, string> root)
        {
			rootFile = root;
        }
    }

    public class DiskFileOutput : HashedFileOutput
	{
		private readonly string rootDirectory;
		private readonly string rootFilePath;

		public DiskFileOutput(string rootFilePath)
		{
			this.rootFilePath = rootFilePath;
			this.rootDirectory = Path.GetDirectoryName(rootFilePath);

			Directory.CreateDirectory(rootDirectory);
		}

		protected override void Delete(string file, string hash)
		{
			File.Delete(Path.Combine(rootDirectory, hash));
		}

		protected override Dictionary<string, string> LoadRootFile()
		{
			if (!File.Exists(rootFilePath))
				return null;

			var raw = File.ReadAllBytes(rootFilePath);

			return JsonSerializer.Deserialize<Dictionary<string, string>>(raw);
		}

		protected override void WriteFile(string file, string hash, string? oldHash, Stream sourceStream)
		{
			using var targetStream = File.OpenWrite(Path.Combine(rootDirectory, hash));
			sourceStream.CopyTo(targetStream);
			targetStream.Close();
		}

		protected override void WriteRootFile(Dictionary<string, string> root)
		{
			var json = JsonSerializer.SerializeToUtf8Bytes(root, new JsonSerializerOptions { WriteIndented = true });

			File.WriteAllBytes(rootFilePath, json);
		}
	}
}
