namespace MK94.Assert.NUnit
{
    public static class SetupDiskAssert
    {
        /// <summary>
        /// Sets the default <see cref="DiskAsserter"/> and only adds a common build agent check
        /// </summary>
        /// <returns>An instance of <see cref="DiskAsserter"/> with basic settings.</returns>
        public static DiskAsserter WithBasicSettings()
        {
            DiskAsserter.Default = InstanceWithBasicSettings();

            return DiskAsserter.Default;
        }

        /// <summary>
        /// Sets the default and fully initialises <see cref="DiskAsserter"/>
        /// </summary>
        /// <returns>A ready to use <see cref="DiskAsserter"/></returns>
        public static DiskAsserter WithRecommendedSettings(string solutionFolder, string outputFolder = "TestData")
        {
            DiskAsserter.Default = InstanceWithRecommendedSettings(solutionFolder, outputFolder);

            return DiskAsserter.Default;
        }

        /// <summary>
        /// Creates a new <see cref="DiskAsserter"/> and only adds a common build agent check
        /// </summary>
        /// <returns>An instance of <see cref="DiskAsserter"/> with basic settings.</returns>
        public static DiskAsserter InstanceWithBasicSettings()
        {
            var ret = new DiskAsserter()
                .WithCommonBuildAgentsCheck();
            ret.PathResolver = new NUnitPathResolver();

            return ret;
        }

        /// <summary>
        /// Creates a new and fully initialises <see cref="DiskAsserter"/> as an instance.
        /// </summary>
        /// <returns>A ready to use <see cref="DiskAsserter"/> instance.</returns>
        public static DiskAsserter InstanceWithRecommendedSettings(string solutionFolder, string outputFolder = "TestData")
        {
            var ret = new DiskAsserter()
                .WithRecommendedSettings(solutionFolder, outputFolder);

            ret.PathResolver = new NUnitPathResolver();

            return ret;
        }
    }
}
