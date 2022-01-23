using MK94.Assert.Output;
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
        static PathResolver pathResolver;
        static ITestOutput fileOutput;

        static List<Func<string, string>> postProcessors = new List<Func<string, string>>();
        static bool WriteMode;

        /// <summary>
        /// Safety flag to avoid checking in <see cref="EnableWriteMode"/> by accident <br />
        /// False by default; Set to true on Dev environments (recommended way is via environment variable)
        /// </summary>
        public static bool IsDevEnvironment { get; set; } = false;

        public static string MatchesRaw(string file, string rawData)
        {
            pathResolver = new PathResolver();
            fileOutput = new HashedFileOutput(new DiskFileOutput(@"C:\Users\shaon\Desktop\github\MK94.Assert\TestData"));

            var outputFile = Path.Combine(pathResolver.GetStepPath(), file);

            if(WriteMode)
            {
                EnsureDevMode();
                fileOutput.Write(outputFile, rawData);

                return rawData;
            }

            if (fileOutput.IsHashMatch(file, rawData))
                return rawData;

            throw new Exception($"Difference in step");
        }

        public static T Matches<T>(T instance, string step)
        {
            var serialized = JsonSerializer.Serialize(instance);

            foreach (var post in postProcessors)
                serialized = post(serialized);

            MatchesRaw($"{step}.json", serialized);

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

        public static void EnsureDevMode()
        {
            if (!IsDevEnvironment)
                throw new InvalidOperationException($"Trying to write during assert but not in a dev environment!!! Make sure EnableWriteMode is not called.");
        }
    }
}
