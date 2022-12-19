using MK94.Assert.Input;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MK94.Assert
{
    /// <summary>
    /// Helper class to call <see cref="DiskAssert"/> methods via its default instance
    /// </summary>
    public static class DiskAssert
    {
        private static AsyncLocal<DiskAsserter> currentInstance = new AsyncLocal<DiskAsserter>();

        /// <summary>
        /// The default <see cref="DiskAsserter"/> instance. <br />
        /// Used by <see cref="DiskAssert"/> and static match methods in <see cref="DiskAssert"/>.
        /// </summary>
        public static DiskAsserter Default
        {
            get
            {
                currentInstance.Value ??= DefaultConfig.Build();


                return currentInstance.Value;
            }
        }

        public static IDiskAsserterConfig DefaultConfig { get; set; }

        /// <inheritdoc cref="DiskAsserter.MatchesRaw(string,string,string,IDifferenceFormatter{string},OperationMode)"/>
        public static string MatchesRaw(string step, string rawData, string fileType = null, IDifferenceFormatter<string> formatter = null) 
            => Default.MatchesRaw(step, rawData, fileType, formatter);

        /// <inheritdoc cref="DiskAsserter.Matches{T}(string, T)"/>
        public static T Matches<T>(string step, T instance) => Default.Matches<T>(step, instance);

        /// <inheritdoc cref="DiskAsserter.Matches{T}(string, Task{T})"/>
        public static Task<T> Matches<T>(string step, Task<T> asyncInstance) => Default.Matches<T>(step, asyncInstance);

        /// <inheritdoc cref="DiskAsserter.MatchesException{T}(string, Task)"/>
        public static Task MatchesException<T>(string step, Task asyncInstance) where T : Exception
            => Default.MatchesException<T>(step, asyncInstance);

        /// <inheritdoc cref="DiskAsserter.Matches{T}(string, Task{T})"/>
        public static Task<T> Matches<T>(this Task<T> asyncInstance, string step)
            => Default.Matches(step, asyncInstance);

        /// <inheritdoc cref="DiskAsserter.MatchesException{T}(string, Task)"/>
        public static Task MatchesException<T>(this Task asyncInstance, string step) where T : Exception
            => Default.MatchesException<T>(step, asyncInstance);

        /// <inheritdoc cref="DiskAsserter.EnableWriteMode"/>
        public static void EnableWriteMode() => Default.EnableWriteMode();

        /// <inheritdoc cref="DiskAsserter.MatchesSequence"/>
        public static void MatchesSequence() => Default.MatchesSequence();

        /// <inheritdoc cref="Extensions.WithInputs"/>
        public static TestInput WithInputs() => Default.WithInputs();
    }
}
