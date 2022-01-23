﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
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

	public class HashedFileOutput : ITestOutput
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

		private static object writeLock = new object();
		private static Dictionary<string, string> rootFile;
		private static ConcurrentDictionary<string, byte[]> readCache = new ConcurrentDictionary<string, byte[]>();

		private readonly IFileOutput baseOutput;

		public HashedFileOutput(IFileOutput baseOutput)
        {
			this.baseOutput = baseOutput;
        }

		private Dictionary<string, string> LoadRootFile()
        {
			if (rootFile != null)
				return rootFile;

			var reader = baseOutput.OpenRead("root.json");

			if (reader == null)
				rootFile = new Dictionary<string, string>();
			else
				rootFile = JsonSerializer.DeserializeAsync<Dictionary<string, string>>(reader).Result;

			return rootFile;
        }

		public bool IsHashMatch(string path, string rawData)
		{
			var root = LoadRootFile();
			var hash = new SHA256Managed();

			var o = new CryptoStream(new MemoryStream(), hash, CryptoStreamMode.Write);

			using var writer = new StreamWriter(o);
			writer.Write(rawData);
			writer.Flush();
			writer.Close();

			return root != null && root.TryGetValue(path, out var existinghash) && existinghash.Equals(HashToString(hash.Hash));
		}

		private string HashToString(byte[] hash)
		{
			return Convert.ToBase64String(hash!).Replace('/', '-').ToLower();
		}

		public Stream OpenRead(string path, bool cache)
		{
			if (readCache.TryGetValue(path, out var buffer))
				return new MemoryStream(buffer, false);

			var ret = baseOutput.OpenRead(path);

			if (!cache)
				return ret;

			buffer = new byte[ret.Length];
			ret.Read(buffer);
			readCache.TryAdd(path, buffer);

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

			lock (writeLock)
			{
				var root = LoadRootFile() ?? new Dictionary<string, string>();

				// TODO containsValue is slow
				if (root.ContainsValue(path))
					return;

				ms.Position = 0;

				var hashAsString = HashToString(hash.Hash);

				baseOutput.Write(hashAsString, ms);

				root[path] = hashAsString;
				WriteRootFile(root);
			}
		}

		private void WriteRootFile(Dictionary<string, string> root)
		{
			baseOutput.Write("root.json", new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(root)));
		}
	}
}
