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
        public static IDiskAsserterConfig WithRecommendedSettings(this IDiskAsserterConfig diskAsserter, string solutionFolder, string outputFolder = "TestData")
        {
            return diskAsserter
                .WithClassTestFolderStructure(solutionFolder, outputFolder)
                .WithCommonBuildAgentsCheck()
                .WithPseudoRandom();
        }

        /// <summary>
        /// Adds safety check for common build agents
        /// </summary>
        /// <param name="diskAsserter"></param>
        /// <returns></returns>
        public static IDiskAsserterConfig WithCommonBuildAgentsCheck(this IDiskAsserterConfig diskAsserter)
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
        public static IDiskAsserterConfig WithPseudoRandom(this IDiskAsserterConfig diskAsserter)
        {
            diskAsserter.SeedGenerator = () => Path.Combine(TestContext.CurrentContext.Test.ClassName, TestContext.CurrentContext.Test.Name);

            return diskAsserter;
        }

        /// <summary>
        /// Adds a check to see if we are running on a developer machine.
        /// Prevents <see cref="DiskAsserter.EnableWriteMode"/> from being accidentally checked in
        /// </summary>
        public static IDiskAsserterConfig WithDevModeOnEnvironmentVariable(this IDiskAsserterConfig diskAsserter, string environmentVariable, string valueOnProd)
        {
            diskAsserter.IsDevEnvironment = Environment.GetEnvironmentVariable(environmentVariable) != valueOnProd;

            return diskAsserter;
        }

        /// <summary>
        /// Stores files as their hash values rather than path. <br />
        /// A root.json file is used to map file paths to actual files. <br />
        /// This avoids redundant files from tests. <br />
        /// </summary>
        public static IDiskAsserterConfig WithOutputInMemory(this IDiskAsserterConfig diskAsserter, out Output.MemoryFileOutput memoryFileOutput)
        {
            memoryFileOutput = new Output.MemoryFileOutput();
            diskAsserter.Output = new Output.DirectTestOutput(memoryFileOutput);

            return diskAsserter;
        }

        /// <summary>
        /// Stores files as their hash values rather than path. <br />
        /// A root.json file is used to map file paths to actual files. <br />
        /// This avoids redundant files from tests. <br />
        /// </summary>
        public static IDiskAsserterConfig WithDeduplication(this IDiskAsserterConfig diskAsserter, string solutionFolder, string folder = "TestData")
        {
            var diskOutput = new Output.DiskFileOutput(PathHelper.PathRelativeToParentFolder(solutionFolder, folder));
            diskAsserter.Output = new Output.HashedTestOutput(diskOutput);

            return diskAsserter;
        }

        /// <inheritdoc cref="WithDeduplication(DiskAsserter, string, string)"/>
        public static IDiskAsserterConfig WithDeduplicationInMemory(this IDiskAsserterConfig diskAsserter, out Output.MemoryFileOutput output)
        {
            output = new Output.MemoryFileOutput();

            return diskAsserter.WithDeduplicationInMemory(output);
        }

        /// <inheritdoc cref="WithDeduplication(DiskAsserter, string, string)"/>
        public static IDiskAsserterConfig WithDeduplicationInMemory(this IDiskAsserterConfig diskAsserter, Output.MemoryFileOutput output)
        {
            diskAsserter.Output = new Output.HashedTestOutput(output);

            return diskAsserter;
        }

        /// <summary>
        /// Stores files in a Class/Test/Step structure
        /// </summary>
        public static IDiskAsserterConfig WithClassTestFolderStructure(this IDiskAsserterConfig diskAsserter, string solutionFolder, string folder = "TestData")
        {
            var diskOutput = new Output.DiskFileOutput(PathHelper.PathRelativeToParentFolder(solutionFolder, folder));
            diskAsserter.Output = new Output.DirectTestOutput(diskOutput);

            return diskAsserter;
        }

        /// <summary>
        /// Sets the serializer to be used for writing when <see cref="DiskAssert.Matches"/> or related methods are called <br />
        /// Sets the serializer to be used for reading when <see cref="TestChainer.Read"/> or related methods are called
        /// </summary>
        public static IDiskAsserterConfig WithSerializer(this IDiskAsserterConfig diskAsserter, ISerializer serializer)
        {
            diskAsserter.Serializer = serializer;

            return diskAsserter;
        }

        /// <summary>
        /// Sets the serializer to be used for writing when <see cref="DiskAssert.Matches"/> or related methods are called <br />
        /// Does <b>not</b> support read operations required for <see cref="TestChainer"/>. Call <see cref="WithSerializer(DiskAsserter, ISerializer)"/> instead!!!
        /// </summary>
        public static IDiskAsserterConfig WithSerializer(this IDiskAsserterConfig diskAsserter, Func<object, string> serializer)
        {
            diskAsserter.Serializer = new SerializeOnlyFunc(serializer);

            return diskAsserter;
        }
    }
}
