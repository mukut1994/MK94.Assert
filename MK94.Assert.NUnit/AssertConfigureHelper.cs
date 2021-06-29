using NUnit.Framework;
using System;
using System.IO;

namespace MK94.Assert.NUnit
{
    public static class AssertConfigureHelper
    {
        /// <inheritdoc cref="AssertConfigure.PathRelativeToParentFolder"/>
        public static Configure WithBaseFolderRelativeToBinary(string relativeParentFolder, string parentRelativeChild)
        {
            AssertConfigure.GlobalPath = AssertConfigure.PathRelativeToParentFolder(relativeParentFolder, parentRelativeChild);

            return new Configure();
        }

        /// <summary>
        /// Configures <see cref="Assert"/> to use class and test name for the folder structure and <see cref="PseudoRandom"/> <br />
        /// Also adds checks for some common CI gates: <br />
        /// - Azure, Github, Gitlab, TeamCity, Octopus, Jenkins
        /// </summary>
        /// <param name="projectRootName"></param>
        /// <param name="testDataPath"></param>
        public static void WithRecommendedSettings(string projectRootName, string testDataPath)
        {
            WithBaseFolderRelativeToBinary(projectRootName, testDataPath)
                .WithChecksumStructure(BasedOn.ClassNameTestName)
                .WithFolderStructure(BasedOn.ClassNameTestName)
                .WithPseudoRandom(BasedOn.ClassNameTestName);

            // Common build agent checks

            if (
                    Environment.GetEnvironmentVariable("Agent.Id") == null && // Azure
                    Environment.GetEnvironmentVariable("CI") != "true" && // Github, Gitlab
                    Environment.GetEnvironmentVariable("teamcity.version") == null && // TeamCity
                    Environment.GetEnvironmentVariable("Octopus.Release.Id") == null && // Octopus
                    Environment.GetEnvironmentVariable("JENKINS_URL") == null // Jenkins
                )
                AssertConfigure.IsDevEnvironment = true;
        }

    }

    public enum BasedOn
    {
        TestName,
        ClassNameTestName
    }

    /// <summary>
    /// Helper class to just make <see cref="AssertConfigureHelper"/> a builder like class
    /// </summary>
    public class Configure
    {
        internal Configure() { }

        public Configure WithFolderStructure(BasedOn basedOn)
        {
            AssertConfigure.AddPathResolver(x => BasedOnPath(basedOn));

            return this;
        }

        public Configure WithChecksumStructure(BasedOn basedOn)
        {
            AssertConfigure.AddChecksumFileResolver(x => Path.Combine(BasedOnPath(basedOn), "checksum"));

            return this;
        }

        public Configure WithPseudoRandom(BasedOn basedOn)
        {
            PseudoRandom.WithBaseSeed(() => BasedOnPath(basedOn));

            return this;
        }

        public Configure WithDevModeOnEnvironmentVariable(string environmentVariable, string valueOnProd)
        {
            AssertConfigure.IsDevEnvironment = Environment.GetEnvironmentVariable(environmentVariable) != valueOnProd;

            return this;
        }

        private static string BasedOnPath(BasedOn basedOn)
        {
            if (basedOn == BasedOn.TestName)
                return TestContext.CurrentContext.Test.MethodName;
            else if (basedOn == BasedOn.ClassNameTestName)
                return Path.Combine(TestContext.CurrentContext.Test.ClassName, TestContext.CurrentContext.Test.MethodName);

            throw new NotImplementedException(basedOn.ToString());
        }
    }
}
