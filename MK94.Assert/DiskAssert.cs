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
    public class DiskAssert
    {
        /// <summary>
        /// The default <see cref="DiskAssert"/> instance. <br />
        /// Used by <see cref="DiskAssertExtensions"/>.
        /// </summary>
        public static DiskAssert Default { get; set; } = new DiskAssert();

        private PathResolver pathResolver;
        private ITestOutput fileOutput;

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
        public string MatchesRaw(string step, string rawData)
        {
            var outputFile = Path.Combine(pathResolver.GetStepPath(), step);

            if (WriteMode)
            {
                EnsureDevMode();
                fileOutput.Write(outputFile, rawData);

                return rawData;
            }

            if (fileOutput.IsHashMatch(step, rawData))
                return rawData;

            throw new Exception($"Difference in step");
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
            var serialized = JsonSerializer.Serialize(instance);

            foreach (var post in postProcessors)
                serialized = post(serialized);

            MatchesRaw($"{step}.json", serialized);

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
                var cleanedStackTrace = Regex.Replace(e.StackTrace, "at (.+)( in (.+))", "$1");

                MatchesRaw(e.Message + Environment.NewLine + cleanedStackTrace, step);
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
    }

    public static class DiskAssertExtensions
    {
        public static Task<T> Matches<T>(this Task<T> asyncInstance, string step) 
            => DiskAssert.Default.Matches(step, asyncInstance);

        public static Task MatchesException<T>(this Task asyncInstance, string step) where T : Exception
            => DiskAssert.Default.MatchesException<T>(step, asyncInstance);
    }
}
