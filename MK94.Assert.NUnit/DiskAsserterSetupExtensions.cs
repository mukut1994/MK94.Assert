using NUnit.Framework;
using System;
using System.IO;

namespace MK94.Assert.NUnit
{
    public static class DiskAsserterSetupExtensions
    {
        /// <summary>
        /// Configures the diskAsserter for common build agents and a pseudo random generator seed
        /// </summary>
        public static DiskAsserter WithRecommendedSettings(this DiskAsserter diskAsserter, string solutionFolder, string outputFolder = "TestData")
        {
            return diskAsserter
                .WithDeduplication(solutionFolder, outputFolder)
                .WithCommonBuildAgentsCheck()
                .WithPseudoRandom();
        }

        /// <summary>
        /// Adds safety check for common build agents
        /// </summary>
        /// <param name="diskAsserter"></param>
        /// <returns></returns>
        public static DiskAsserter WithCommonBuildAgentsCheck(this DiskAsserter diskAsserter)
        {
            if (
                Environment.GetEnvironmentVariable("Agent.Id") == null && // Azure
                Environment.GetEnvironmentVariable("CI") != "true" && // Github, Gitlab
                Environment.GetEnvironmentVariable("teamcity.version") == null && // TeamCity
                Environment.GetEnvironmentVariable("Octopus.Release.Id") == null && // Octopus
                Environment.GetEnvironmentVariable("JENKINS_URL") == null // Jenkins
            )
                diskAsserter.IsDevEnvironment = true;

            return diskAsserter;
        }

        /// <summary>
        /// Initialises the <see cref="PseudoRandom"/> class with a seed generator
        /// </summary>
        public static DiskAsserter WithPseudoRandom(this DiskAsserter diskAsserter)
        {
            PseudoRandom.WithBaseSeed(() => Path.Combine(TestContext.CurrentContext.Test.ClassName, TestContext.CurrentContext.Test.Name));

            return diskAsserter;
        }

        /// <summary>
        /// Adds a check to see if we are running on a developer machine.
        /// Prevents <see cref="DiskAsserter.EnableWriteMode"/> from being accidentally checked in
        /// </summary>
        public static DiskAsserter WithDevModeOnEnvironmentVariable(this DiskAsserter diskAsserter, string environmentVariable, string valueOnProd)
        {
            diskAsserter.IsDevEnvironment = Environment.GetEnvironmentVariable(environmentVariable) != valueOnProd;

            return diskAsserter;
        }

        /// <summary>
        /// Stores files as their hash values rather than path. <br />
        /// A root.json file is used to map file paths to actual files. <br />
        /// This avoids redundant files from tests. <br />
        /// </summary>
        public static DiskAsserter WithDeduplication(this DiskAsserter diskAsserter, string solutionFolder, string folder = "TestData")
        {
            var diskOutput = new Output.DiskFileOutput(PathHelper.PathRelativeToParentFolder(solutionFolder, folder));
            diskAsserter.output = new Output.HashedFileOutput(diskOutput);

            return diskAsserter;
        }

        /// <inheritdoc cref="WithDeduplication(DiskAsserter, string, string)"/>
        public static DiskAsserter WithDeduplicationInMemory(this DiskAsserter diskAsserter, out Output.MemoryFileOutput output)
        {
            output = new Output.MemoryFileOutput();

            return diskAsserter.WithDeduplicationInMemory(output);
        }

        /// <inheritdoc cref="WithDeduplication(DiskAsserter, string, string)"/>
        public static DiskAsserter WithDeduplicationInMemory(this DiskAsserter diskAsserter, Output.MemoryFileOutput output)
        {
            diskAsserter.output = new Output.HashedFileOutput(output);

            return diskAsserter;
        }

        // TODO 
        private static DiskAsserter WithFolderStructure(this DiskAsserter diskAsserter/*BasedOn basedOn*/)
        {
            //AssertConfigure.AddPathResolver(x => BasedOnPath(basedOn));

            return diskAsserter;
        }
    }
}
