using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MK94.Assert.Output
{
	public interface ITestOutput
	{
		Stream OpenRead(string path, bool cache);

		bool IsHashMatch(string path, string rawData);

		void Write(string path, string rawData);
	}

	public interface IFileOutput
	{
		Stream OpenRead(string path);

		void Write(string path, Stream sourceStream);

		void Delete(string path);
	}

	internal static class TestOutputHelper
	{
		public static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

		public static string HashToString(byte[] hash)
		{
			return Convert.ToBase64String(hash!).Replace('/', '-').ToLower();
		}
		
		public static Dictionary<string, string> LoadRootFile(Dictionary<string, string> rootFile, IFileOutput baseOutput)
		{
			if (rootFile != null)
				return rootFile;

			using var reader = baseOutput.OpenRead("root.json");

			if (reader == null)
				rootFile = new Dictionary<string, string>();
			else
				rootFile = JsonSerializer.DeserializeAsync<Dictionary<string, string>>(reader).Result;

			return rootFile;
		}
	}

	public class HashedTestOutput : ITestOutput
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

		private static readonly object WriteLock = new object();
		private static readonly ConcurrentDictionary<string, byte[]> ReadCache = new ConcurrentDictionary<string, byte[]>();

		private Dictionary<string, string> rootFile;

		private readonly IFileOutput baseOutput;

		public HashedTestOutput(IFileOutput baseOutput)
        {
			this.baseOutput = baseOutput;
        }

		private Dictionary<string, string> LoadRootFile()
        {
			return TestOutputHelper.LoadRootFile(rootFile, baseOutput);
        }

		public bool IsHashMatch(string path, string rawData)
		{
			path = path.Replace('\\', '/');

			var root = LoadRootFile();
			var hash = new SHA256Managed();

			var o = new CryptoStream(new MemoryStream(), hash, CryptoStreamMode.Write);

			using var writer = new StreamWriter(o);
			writer.Write(rawData);
			writer.Flush();
			writer.Close();

			return root != null && root.TryGetValue(path, out var existingHash) && existingHash.Equals(TestOutputHelper.HashToString(hash.Hash));
		}

		public Stream OpenRead(string path, bool cache)
		{
			if (ReadCache.TryGetValue(path, out var buffer))
				return new MemoryStream(buffer, false);

			var root = LoadRootFile();

			// Replace windows path / with \
			var actualPath = root.GetValueOrDefault(path.Replace('\\', '/'));

			if (actualPath == null)
				return null;

			var ret = baseOutput.OpenRead(actualPath);

			if (!cache)
				return ret;

			buffer = new byte[ret.Length];
			ret.Read(buffer);
			ReadCache.TryAdd(path, buffer);

			return new MemoryStream(buffer, false);
		}

		public void Write(string path, string rawData)
		{
			var ms = new MemoryStream();
			using var hash = new SHA256Managed();
			using var cs = new CryptoStream(ms, hash, CryptoStreamMode.Write, true);
			using var writer = new StreamWriter(cs);

			writer.Write(rawData);
			writer.Flush();
			writer.Close();
			cs.Close();

			lock (WriteLock)
			{
				var root = LoadRootFile() ?? new Dictionary<string, string>();

				var hashAsString = TestOutputHelper.HashToString(hash.Hash);

				var duplicateFileExists = root.ContainsValue(TestOutputHelper.HashToString(hash.Hash));

				// Replace windows path / with \
				root[path.Replace('\\', '/')] = hashAsString;

				WriteRootFile(root);

				// TODO containsValue is slow; but maybe this is fine? this shouldn't be called very often
				if (duplicateFileExists)
					return;

				ms.Position = 0;
				baseOutput.Write(hashAsString, ms);
			}
		}

		private void WriteRootFile(Dictionary<string, string> root)
		{
			baseOutput.Write("root.json", new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(root, TestOutputHelper.JsonSerializerOptions)));
		}
	}
}
