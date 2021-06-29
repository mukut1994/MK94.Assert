using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MK94.Assert
{
    public static class DiskAssert
    {
        static JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

        public static string MatchesRaw(string rawData, string step)
        {
            var basePath = AssertConfigure.GlobalPath ?? AssertConfigure.DefaultGlobalPath;
            string outputFile = null;

            if (AssertConfigure.PathResolver != null)
            {
                outputFile = Path.GetFullPath(Path.Combine(basePath, AssertConfigure.PathResolver(new AssertContext(step)), step + ".json"));
                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
            }
            else
            {
                Directory.CreateDirectory(basePath);
                outputFile = Path.Combine(step + ".json");
            }


            if (AssertConfigure.WriteMode)
            {
                AssertConfigure.EnsureDevMode();

                File.WriteAllText(outputFile, rawData);
            }
            else
            {
                if (!File.Exists(outputFile))
                    throw new FileNotFoundException($"Could not find file '{outputFile}'. Is this a new test? Enable {nameof(AssertConfigure.WriteMode)} in {nameof(AssertConfigure)} to fix");

                if (!File.ReadAllText(outputFile).Equals(rawData))
                    throw new InvalidProgramException($"Difference in step {step}"); // TODO better error, with where and what
            }

            return rawData;
        }

        public static T Matches<T>(T instance, string step)
        {
            var serialized = JsonSerializer.Serialize(instance, options);

            MatchesRaw(serialized, step);

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
