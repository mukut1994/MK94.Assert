using MK94.Assert.Output;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MK94.Assert
{
    public class DiskAsserter
    {
        /// <summary>
        /// The default <see cref="DiskAsserter"/> instance. <br />
        /// Used by <see cref="DiskAssertExtensions"/> and static match methods in <see cref="DiskAssertStatic"/>.
        /// </summary>
        public static DiskAsserter Default { get; set; }
        
        private static JsonSerializerOptions serializerOptions = new JsonSerializerOptions { WriteIndented = true };

        public IPathResolver pathResolver;
        public ITestOutput output;

        private List<Func<string, string>> postProcessors = new List<Func<string, string>>();
        private bool WriteMode = false;

        /// <summary>
        /// Safety flag to avoid checking in <see cref="EnableWriteMode"/> by accident <br />
        /// False by default; Set to true on Dev environments (recommended way is via environment variable)
        /// </summary>
        public bool IsDevEnvironment { get; set; } = false;

        /// <summary>
        /// Checks if a text file matches raw text without any serialization or post processing
        /// </summary>
        /// <param name="step">A descriptive name of the step that generated the data</param>
        /// <param name="rawData">The raw data to be compared</param>
        /// <returns>The unmodified <paramref name="rawData"/></returns>
        /// <exception cref="Exception">Thrown when some differences have been detected</exception>
        public string MatchesRaw(string step, string rawData, string fileType = null, IDifferenceFormatter<string> formatter = null)
        {
            EnsureSetupWasCalled();

            var outputFile = Path.Combine(pathResolver.GetStepPath(), fileType != null ? $"{step}.{fileType}" : step);

            if (WriteMode)
            {
                EnsureDevMode();
                output.Write(outputFile, rawData);

                return rawData;
            }

            if (output.IsHashMatch(outputFile, rawData))
                return rawData;

            ThrowDifferences(step, rawData, formatter, outputFile);
            return null;
        }

        private void ThrowDifferences(string step, string rawData, IDifferenceFormatter<string> formatter, string outputFile)
        {
            using var file = output.OpenRead(outputFile, false);

            if (file == null)
                 throw new Exception($"Missing file {outputFile}; Is this a new test?");

            using var reader = new StreamReader(file);

            if (formatter == null)
                throw new Exception($"Difference in step {step}; Expected {reader?.ReadToEnd() ?? "null"}; Actual: {rawData}");

            var differences = formatter.FindDifferences(this, reader.ReadToEnd(), rawData).ToList();
            var errorBuilder = new StringBuilder();
            errorBuilder.AppendLine($"Difference in step {step}");

            foreach (var diff in differences)
            {
                errorBuilder.AppendLine($"At {diff.Location}: Expected '{diff.Expected}'; Actual: '{diff.Actual}'");
            }

            throw new DifferenceException(errorBuilder.ToString(), differences);
        }

        /// <summary>
        /// Checks if an object matches a previous run
        /// </summary>
        /// <param name="step">A descriptive name of the step that generated the data</param>
        /// <param name="instance">The object to serialise and match</param>
        /// <returns>The unmodified <paramref name="instance"/></returns>
        /// <exception cref="Exception">Thrown when some differences have been detected</exception>
        public T Matches<T>(string step, T instance)
        {
            var serialized = JsonSerializer.Serialize(instance, serializerOptions);

            foreach (var post in postProcessors)
                serialized = post(serialized);

            MatchesRaw($"{step}", serialized, "json", JsonDifferenceFormatter.Instance);

            return instance;
        }

        /// <inheritdoc cref="Matches{T}(string, T)"/>
        public async Task<T> Matches<T>(string step, Task<T> asyncInstance)
        {
            var instance = await asyncInstance;

            return Matches(step, instance);
        }

        /// <summary>
        /// Checks if a task throws an expected exception
        /// </summary>
        /// <param name="step">A descriptive name of the step that generates the exception</param>
        /// <param name="asyncInstance">The pending task to watch for exceptions</param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        /// <exception cref="Exception">Thrown when some differences have been detected</exception>
        public async Task MatchesException<T>(string step, Task asyncInstance)
            where T : Exception
        {
            try
            {
                await asyncInstance;
            }
            catch (T e)
            {
                // remove "in {file}:line{num}" from stack trace
                // a) they change very often because of code refactors
                // b) they contain machine specific folder paths
                var cleanedStackTrace = Regex.Replace(e.StackTrace, "at (.+)( in (.+))", "$1").Replace("\r\n", "\n");

                MatchesRaw(step, e.Message + "\n" + cleanedStackTrace);
            }
        }

        /// <summary>
        /// Safety method to avoid running code in CI/CD environments
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when environment is not a Dev machine</exception>
        public void EnsureDevMode()
        {
            if (!IsDevEnvironment)
                throw new InvalidOperationException($"Trying to write during assert but not in a dev environment!!! Make sure EnableWriteMode is not called.");
        }

        /// <summary>
        /// Changes any calls to <see cref="DiskAsserter.Matches{T}(string, T)"/> and related methods to write to disk instead of comparing
        /// </summary>
        public DiskAsserter EnableWriteMode()
        {
            EnsureDevMode();
            WriteMode = true;

            return this;
        }

        /// <summary>
        /// Changes any calls to <see cref="DiskAsserter.Matches{T}(string, T)"/> and related methods to compare instead of writing to disk
        /// </summary>
        public DiskAsserter DisableWriteMode()
        {
            WriteMode = false;

            return this;
        }

        private void EnsureSetupWasCalled()
        {
            if (pathResolver == null || output == null)
                throw new InvalidOperationException($"DiskAsserter is not fully setup. Call DiskAsserter.WithRecommendedDefaults() first");
        }
    }

    /// <summary>
    /// Helper class to call <see cref="DiskAssert"/> methods via its default instance
    /// </summary>
    public static class DiskAssert
    {
        /// <inheritdoc cref="DiskAsserter.MatchesRaw(string, string)"/>
        public static string MatchesRaw(string step, string rawData) => DiskAsserter.Default.MatchesRaw(step, rawData);

        /// <inheritdoc cref="DiskAsserter.Matches{T}(string, T)"/>
        public static T Matches<T>(string step, T instance) => DiskAsserter.Default.Matches<T>(step, instance);

        /// <inheritdoc cref="DiskAsserter.Matches{T}(string, Task{T})"/>
        public static Task<T> Matches<T>(string step, Task<T> asyncInstance) => DiskAsserter.Default.Matches<T>(step, asyncInstance);

        /// <inheritdoc cref="DiskAsserter.MatchesException{T}(string, Task)"/>
        public static Task MatchesException<T>(string step, Task asyncInstance) where T : Exception
            => DiskAsserter.Default.MatchesException<T>(step, asyncInstance);

        /// <inheritdoc cref="DiskAsserter.Matches{T}(string, Task{T})"/>
        public static Task<T> Matches<T>(this Task<T> asyncInstance, string step)
            => DiskAsserter.Default.Matches(step, asyncInstance);

        /// <inheritdoc cref="DiskAsserter.MatchesException{T}(string, Task)"/>
        public static Task MatchesException<T>(this Task asyncInstance, string step) where T : Exception
            => DiskAsserter.Default.MatchesException<T>(step, asyncInstance);

        /// <inheritdoc cref="DiskAsserter.EnableWriteMode"/>
        public static void EnableWriteMode() => DiskAsserter.Default.EnableWriteMode();
    }
}
