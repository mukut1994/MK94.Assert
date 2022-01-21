using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MK94.Assert
{
    public static class DiskAssert
    {
        static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> checksums = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        static readonly SHA512 sha = SHA512.Create();
        
        public static string MatchesRaw(string rawData, string step, string fileType = "raw")
        {
            var context = new AssertContext(step);
            var basePath = AssertConfigure.GlobalPath ?? AssertConfigure.DefaultGlobalPath;
            string outputFile;

            if (AssertConfigure.PathResolver != null)
            {
                outputFile = Path.GetFullPath(Path.Combine(basePath, AssertConfigure.PathResolver(context), $"{step}.{fileType}"));
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
            }
            else
            {
                Directory.CreateDirectory(basePath);
                outputFile = Path.Combine(basePath, $"{step}.{fileType}");
            }
            
            if (AssertConfigure.WriteMode)
            {
                AssertConfigure.EnsureDevMode();

                if (ChecksumMatches(rawData, outputFile, context))
                    return rawData; // The file already matches; don't write to disk and save a few write cycles

                WriteChecksumFile(rawData, outputFile, context);
                File.WriteAllText(outputFile, rawData);

                return rawData;
            }

            if (!File.Exists(outputFile))
                throw new FileNotFoundException($"Could not find file '{outputFile}'. Is this a new test? Enable {nameof(AssertConfigure.WriteMode)} in {nameof(AssertConfigure)} to fix");

            if (ChecksumMatches(rawData, outputFile, context))
                return rawData;

            if (!File.ReadAllText(outputFile).Equals(rawData))
                throw new InvalidProgramException($"Difference in step {step}, {File.ReadAllText(outputFile)}, AND {rawData}"); // TODO better error, with where and what

            return rawData;
        }

        private static bool ChecksumMatches(string raw, string outputFile, AssertContext context)
        {
            if (AssertConfigure.ChecksumFileResolver == null)
                return false;

            var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(raw));

            var checksumFilePath = GetChecksumFilePath(context);

            var checksumKey = Path.GetRelativePath(checksumFilePath, outputFile);

            var checksumDict = checksums.GetOrAdd(checksumFilePath, ReadChecksumFile);

            if (!checksumDict.TryGetValue(checksumKey, out var checksum))
                return false;

            if (checksum != Convert.ToBase64String(hash))
                return false;

            return true;
        }

        private static string GetChecksumFilePath(AssertContext context)
        {
            var basePath = AssertConfigure.GlobalPath ?? AssertConfigure.DefaultGlobalPath;
            return Path.Combine(basePath, AssertConfigure.ChecksumFileResolver(context));
        }

        private static void WriteChecksumFile(string raw, string outputFile, AssertContext context)
        {
            var checksumFilePath = GetChecksumFilePath(context);

            Directory.CreateDirectory(Path.GetDirectoryName(checksumFilePath));

            var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(raw));

            var checksumKey = Path.GetRelativePath(checksumFilePath, outputFile);

            var checksumDict = checksums.GetOrAdd(checksumFilePath, ReadChecksumFile);

            checksumDict[checksumKey] = Convert.ToBase64String(hash);

            var serialized = JsonSerializer.Serialize(checksumDict);

            File.WriteAllText(checksumFilePath, serialized);
        }

        private static ConcurrentDictionary<string, string> ReadChecksumFile(string path)
        {
            if (!File.Exists(path))
                return new ConcurrentDictionary<string, string>();

            return JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(File.ReadAllText(path));
        }

        public static T Matches<T>(T instance, string step)
        {
            var serialized = JsonSerializer.Serialize(instance);

            foreach (var post in AssertConfigure.postProcessors)
                serialized = post(serialized);

            MatchesRaw(serialized, step, "json");

            return instance;
        }

        public static async Task<T> Matches<T>(this Task<T> asyncInstance, string step)
        {
            var instance = await asyncInstance;

            return Matches(instance, step);
        }

        public static async Task MatchesException<T>(this Task asyncInstance, string step)
            where T : Exception
        {
            try
            {
                await asyncInstance;
            }
            catch(T e)
            {
                // remove "in {file}:line{num}" from stack trace
                // a) they change very often because of code refactors
                // b) they contain machine specific folder paths
                var cleanedStackTrace = Regex.Replace(e.StackTrace, "at (.+)( in (.+))", "$1");

                MatchesRaw(e.Message + Environment.NewLine + cleanedStackTrace, step);
            }
        }
    }
}
