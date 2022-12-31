using MK94.Assert.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MK94.Assert
{
    public class DiskAsserter : IDiskAsserterConfig
    {
        private const string sequenceFile = "_sequence";

        public IPathResolver PathResolver { get; set; }
        public ITestOutput Output { get; set; }
        public ISerializer Serializer { get; set; } = new SystemTextJsonSerializer();
        public Func<string> SeedGenerator { get; set; }

        /// <summary>
        /// The random data generator tied to this <see cref="DiskAsserter"/>.
        /// It's set by <see cref="SeedGenerator"/> during build.
        /// </summary>
        public PseudoRandomizer PseudoRandomizer { get; internal set; }

        private List<AssertOperation> operations { get; } = new List<AssertOperation>();

        /// <summary>
        /// An ordered list of methods that have been called on <see cref="DiskAsserter"/>. Used by <see cref="MatchesSequence"/>
        /// </summary>
        public IReadOnlyList<AssertOperation> Operations => operations;

        public bool WriteMode { get; internal set; } = false;

        public bool InSetup { get; internal set; } = false;

        public bool IsDevEnvironment { get; set; }

        /// <summary>
        /// Read a step from the current test path and add it to the Operations list
        /// </summary>
        /// <param name="step">The step name</param>
        /// <returns>A stream to the step or null if it doesn't exist</returns>
        public Stream Read(string step)
        {
            var ret = Output.OpenRead(step, false);

            if (!InSetup && ret != null)
                operations.Add(new AssertOperation(OperationMode.Input, step));

            return ret;
        }

        /// <summary>
        /// Checks if a text file matches raw text without any serialization or post processing
        /// </summary>
        /// <param name="step">A descriptive name of the step that generated the data</param>
        /// <param name="rawData">The raw data to be compared</param>
        /// <param name="fileType">The file type to save as</param>
        /// <param name="formatter">The error message formatter to use when there are differences</param>
        /// <param name="mode">The mode to add it as to Operations</param>
        /// <returns>The unmodified <paramref name="rawData"/></returns>
        /// <exception cref="Exception">Thrown when some differences have been detected</exception>
        public string MatchesRaw(string step, string rawData, string fileType = null, IDifferenceFormatter<string> formatter = null, OperationMode mode = OperationMode.Output)
        {
            if (InSetup)
                return rawData;

            EnsureSetupWasCalled();

            var outputFile = GetStepPath(step, fileType);

            operations.Add(new AssertOperation(mode, outputFile.Replace('\\', '/')));

            if (WriteMode)
            {
                EnsureDevMode(this);
                Output.Write(outputFile, rawData);

                return rawData;
            }

            if (Output.IsHashMatch(outputFile, rawData))
                return rawData;

            return ThrowDifferences(step, rawData, formatter, outputFile);
        }

        private string ThrowDifferences(string step, string rawData, IDifferenceFormatter<string> formatter, string outputFile)
        {
            using var file = Output.OpenRead(outputFile, false);

            if (file == null)
                 throw new Exception($"Missing file {outputFile}; Is this a new test?");

            using var reader = new StreamReader(file);

            if (formatter == null)
                throw new Exception($"Difference in step {step}; Expected {reader.ReadToEnd()}; Actual: {rawData}");

            var differences = formatter.FindDifferences(reader.ReadToEnd(), rawData).ToList();
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
            var serialized = Serializer.Serialize(instance);

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
        /// Call at the end of tests to make sure the sequence of events stays consistent between test runs
        /// </summary>
        public void MatchesSequence()
        {
            // TODO in write mode also remove unused output files

            Matches(sequenceFile, Operations);
        }

        /// <summary>
        /// Safety method to avoid running code in CI/CD environments
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when environment is not a Dev machine</exception>
        internal static void EnsureDevMode(IDiskAsserterConfig config)
        {
            if (!config.IsDevEnvironment)
                throw new InvalidOperationException($"Trying to write during assert but not in a dev environment!!! Make sure EnableWriteMode is not called.");
        }

        /// <summary>
        /// Safety method to avoid running code in <see cref="DiskAssertSetupExtensions.WithSetup(DiskAsserter, Func{Task})"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when called within a setup context</exception>
        private void EnsureNotSetupMode()
        {
            if (InSetup)
                throw new InvalidOperationException($"Trying to write during setup!!! Make sure EnableWriteMode is not called in methods called by WithSetup.");
        }

        public IDiskAsserterConfig EnableWriteMode()
        {
            EnsureNotSetupMode();
            EnsureDevMode(this);
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

        /// <summary>
        /// Returns all the operations that occurred in a previous test run <br />
        /// Cannot be called in write mode.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public List<AssertOperation> GetOperations()
        {
            if (WriteMode)
                throw new InvalidOperationException($"{nameof(GetOperations)} is not supported when {nameof(WriteMode)} is enabled");

            using var input = Output.OpenRead(Path.Combine(PathResolver.GetStepPath(), sequenceFile + ".json"), true);

            if (input == null)
                throw new InvalidOperationException($"No sequence recorded for current test, is this a new test? Run with EnableWriteMode() and call MatchesSequence() at the end of the test");

            return Serializer.Deserialize<List<AssertOperation>>(input);
        }

        private void EnsureSetupWasCalled()
        {
            if (PathResolver == null || Output == null)
                throw new InvalidOperationException($"DiskAsserter is not fully setup. Call DiskAsserter.WithRecommendedDefaults() first");
        }

        public DiskAsserter Build()
        {
            if(SeedGenerator != null && PseudoRandomizer == null)
                PseudoRandomizer = new PseudoRandomizer(SeedGenerator());

            return this;
        }

        public string GetStepAbsolutePath(string step, string fileType = null)
        {
            return Output.GetAbsolutePathOf(GetStepPath(step, fileType));
        }

        public string GetStepPath(string step, string fileType = null)
        {
            var stepPath = Path.Combine(PathResolver.GetStepPath(), fileType != null ? $"{step}.{fileType}" : step);

            return stepPath;
        }
    }
}
