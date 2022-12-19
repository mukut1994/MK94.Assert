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
            DiskAssert.DefaultConfig = InstanceWithBasicSettings();

            return DiskAssert.Default;
        }

        /// <summary>
        /// Sets the default and fully initialises <see cref="DiskAsserter"/>
        /// </summary>
        /// <returns>A ready to use <see cref="DiskAsserter"/></returns>
        public static IDiskAsserterConfig WithRecommendedSettings(string solutionFolder, string outputFolder = "TestData")
        {
            DiskAssert.DefaultConfig = InstanceWithRecommendedSettings(solutionFolder, outputFolder);

            return DiskAssert.DefaultConfig;
        }

        /// <summary>
        /// Creates a new <see cref="DiskAsserter"/> and only adds a common build agent check
        /// </summary>
        /// <returns>An instance of <see cref="DiskAsserter"/> with basic settings.</returns>
        public static IDiskAsserterConfig InstanceWithBasicSettings()
        {
            var ret = new DiskAsserterConfig()
                .WithCommonBuildAgentsCheck();
            ret.PathResolver = new NUnitPathResolver();

            return ret;
        }

        /// <summary>
        /// Creates a new and fully initialises <see cref="DiskAsserter"/> as an instance.
        /// </summary>
        /// <returns>A ready to use <see cref="DiskAsserter"/> instance.</returns>
        public static IDiskAsserterConfig InstanceWithRecommendedSettings(string solutionFolder, string outputFolder = "TestData")
        {
            var ret = new DiskAsserterConfig()
                .WithRecommendedSettings(solutionFolder, outputFolder);

            ret.PathResolver = new NUnitPathResolver();

            return ret;
        }
    }
}
