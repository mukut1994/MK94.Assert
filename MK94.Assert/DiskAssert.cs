using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        static ConcurrentDictionary<string, ConcurrentDictionary<string, string>> checksums = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        static SHA512 sha = SHA512.Create();
        static JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

        public static string MatchesRaw(string rawData, string step, string fileType = "raw")
        {
            var context = AssertConfigure.GetContext(step);
            string outputFile = AssertConfigure.GetOutputPath(step, fileType, context);

            if (AssertConfigure.WriteMode)
            {
                AssertConfigure.EnsureDevMode();

                if (ChecksumMatches(rawData, outputFile, context))
                    return rawData; // The file already matches; don't write to disk and save a few write cycles

                WriteChecksumFile(rawData, outputFile, context);
                File.WriteAllText(outputFile, rawData);
            }
            else
            {
                if (!File.Exists(outputFile))
                    throw new FileNotFoundException($"Could not find file '{outputFile}'. Is this a new test? Enable {nameof(AssertConfigure.WriteMode)} in {nameof(AssertConfigure)} to fix");

                if (ChecksumMatches(rawData, outputFile, context))
                    return rawData;

                if (!File.ReadAllText(outputFile).Equals(rawData))
                    throw new InvalidProgramException($"Difference in step {step}"); // TODO better error, with where and what
            }

            return rawData;
        }

        
        public static void Cleardown()
        {
            foreach (var checksumDict in checksums)
            {
                var serialized = JsonSerializer.Serialize(checksumDict.Value, options);

                File.WriteAllText(checksumDict.Key, serialized);
            }
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
            if (AssertConfigure.ChecksumFileResolver == null)
                return;

            var checksumFilePath = GetChecksumFilePath(context);

            Directory.CreateDirectory(Path.GetDirectoryName(checksumFilePath));

            var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(raw));

            var checksumKey = Path.GetRelativePath(checksumFilePath, outputFile);

            var checksumDict = checksums.GetOrAdd(checksumFilePath, ReadChecksumFile);

            checksumDict[checksumKey] = Convert.ToBase64String(hash);
        }

        private static ConcurrentDictionary<string, string> ReadChecksumFile(string path)
        {
            if (!File.Exists(path))
                return new ConcurrentDictionary<string, string>();

            return JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(File.ReadAllText(path));
        }

        public static T Matches<T>(T instance, string step)
        {
            var serialized = JsonSerializer.Serialize(instance, options);

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
