using MK94.Assert.Output;
using System;

namespace MK94.Assert
{
    public interface IDiskAsserterConfig
    {
        /// <summary>
        /// The path resolver to determine per test paths. Usually related to which test framework is being used e.g. XUnit, NUnit, MSTest etc
        /// </summary>
        public IPathResolver PathResolver { get; set; }

        /// <summary>
        /// The output strategy for file chunking and hashing. Default is <see cref="DirectTestOutput"/>
        /// </summary>
        public ITestOutput Output { get; set; }

        /// <summary>
        /// The serializer for calls to <see cref="DiskAsserter.Matches{T}(string, T)"/>
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// Safety flag to avoid checking in <see cref="EnableWriteMode"/> by accident <br />
        /// False by default; Set to true on Dev environments (recommended way is via environment variable)
        /// </summary>
        public bool IsDevEnvironment { get; set; }

        /// <summary>
        /// Changes any calls to <see cref="DiskAsserter.Matches{T}(string, T)"/> and related methods to write to disk instead of comparing
        /// </summary>
        public IDiskAsserterConfig EnableWriteMode();

        public Func<string> SeedGenerator { get; set; }

        public DiskAsserter Build();
    }

    public class DiskAsserterConfig : IDiskAsserterConfig
    {
        public IPathResolver PathResolver { get; set; }
        public ITestOutput Output { get; set; }
        public ISerializer Serializer { get; set; }
        public bool IsDevEnvironment { get; set; }
        public bool WriteMode { get; set; }
        public Func<string> SeedGenerator { get; set; }

        public DiskAsserter Build()
        {
            var ret = new DiskAsserter();
            ret.Output = Output;
            ret.IsDevEnvironment = IsDevEnvironment;
            ret.PathResolver = PathResolver;
            ret.PseudoRandomizer = SeedGenerator != null ? new PseudoRandomizer(SeedGenerator()) : null;

            if (WriteMode)
                ret.EnableWriteMode();

            if (Serializer != null)
                ret.Serializer = Serializer;

            return ret;
        }

        public IDiskAsserterConfig EnableWriteMode()
        {
            DiskAsserter.EnsureDevMode(this);

            WriteMode = true;

            return this;
        }
    }
}
