using MK94.Assert.Chain;
using System;
using System.Threading.Tasks;

namespace MK94.Assert
{
    /// <summary>
    /// Helper class to call <see cref="DiskAssert"/> methods via its default instance
    /// </summary>
    public static class DiskAssert
    {
        /// <inheritdoc cref="DiskAsserter.MatchesRaw(string, string, string, IDifferenceFormatter{string})"/>
        public static string MatchesRaw(string step, string rawData, string fileType = null, IDifferenceFormatter<string> formatter = null) 
            => DiskAsserter.Default.MatchesRaw(step, rawData, fileType, formatter);

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

        /// <inheritdoc cref="DiskAsserter.MatchesSequence"/>
        public static void MatchesSequence() => DiskAsserter.Default.MatchesSequence();

        /// <inheritdoc cref="Extensions.WithInputs"/>
        public static TestChainer WithInputs() => DiskAsserter.Default.WithInputs();
    }
}
