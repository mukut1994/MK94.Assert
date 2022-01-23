using System;

namespace MK94.Assert
{
    public static class SetupAssertConfiguration
    {
        /// <inheritdoc cref="AssertConfigure.PathRelativeToParentFolder"/>
        public static T WithBaseFolderRelativeToBinary<T>(string relativeParentFolder, string parentRelativeChild) where T: IConfiguration, new()
        {
            AssertConfigure.GlobalPath = AssertConfigure.PathRelativeToParentFolder(relativeParentFolder, parentRelativeChild);

            return new T();
        }

        /// <summary>
        /// Configures <see cref="DiskAsserter"/> to use class and test name for the folder structure and <see cref="PseudoRandom"/> <br />
        /// Also adds checks for some common CI gates: <br />
        /// - Azure, Github, Gitlab, TeamCity, Octopus, Jenkins
        /// </summary>
        /// <param name="projectRootName"></param>
        /// <param name="testDataPath"></param>
        public static void WithRecommendedSettings<T>() where T: IConfiguration, new()
        {
            new T()
                .WithFolderStructure(BasedOn.ClassNameTestName)
                .WithPseudoRandom(BasedOn.ClassNameTestName)
                .WithChecksumStructure(BasedOn.ClassNameTestName);

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
}