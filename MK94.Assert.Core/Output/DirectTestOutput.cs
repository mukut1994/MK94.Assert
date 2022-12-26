using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace MK94.Assert.Output
{
    public class DirectTestOutput : ITestOutput
    {
        private static readonly object WriteLock = new object();

        private readonly IFileOutput fileOutput;

        private Dictionary<string, string> rootFile;

        public DirectTestOutput(IFileOutput fileOutput)
        {
            this.fileOutput = fileOutput;
        }

        private Dictionary<string, string> LoadRootFile()
        {
            rootFile = TestOutputHelper.LoadRootFile(rootFile, fileOutput);
            return rootFile;
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
            path = path.Replace('\\', '/');

            return fileOutput.OpenRead(path);
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

                // Replace windows path / with \
                root[path.Replace('\\', '/')] = hashAsString;

                WriteRootFile(root);

                ms.Position = 0;
                fileOutput.Write(path, ms);
            }
        }

        private void WriteRootFile(Dictionary<string, string> root)
        {
            fileOutput.Write("root.json", new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(root, TestOutputHelper.JsonSerializerOptions)));
        }

        public string GetAbsolutePathOf(string path)
        {
            return fileOutput.GetAbsolutePathOf(path);
        }
    }
}
