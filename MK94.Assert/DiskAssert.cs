using MK94.Assert.Output;
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
        static PathResolver pathResolver;
        static ITestOutput fileOutput;

        public static string MatchesRaw(string file, string rawData)
        {
            pathResolver = new PathResolver();
            fileOutput = new HashedFileOutput(new DiskFileOutput(@"C:\Users\shaon\Desktop\github\MK94.Assert\TestData"));

            var outputFile = Path.Combine(pathResolver.GetStepPath(), file);

            if(AssertConfigure.WriteMode)
            {
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

            foreach (var post in AssertConfigure.postProcessors)
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
    }
}
