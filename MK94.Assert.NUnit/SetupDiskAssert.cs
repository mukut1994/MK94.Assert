namespace MK94.Assert.NUnit
{
    public static class SetupDiskAssert
    {
        /// <summary>
        /// Sets the default <see cref="DiskAsserter"/> and only adds a common build agent check
        /// </summary>
        /// <returns>An unconfigured <see cref="DiskAsserter"/></returns>
        public static DiskAsserter WithBasicSettings()
        {
            DiskAsserter.Default = InstanceWithBasicSettings();

            return DiskAsserter.Default;
        }

        /// <summary>
        /// Sets the default <see cref="DiskAsserter"/> and fully initialises
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
        /// <returns>An unconfigured <see cref="DiskAsserter"/></returns>
        public static DiskAsserter InstanceWithBasicSettings()
        {
            var ret = new DiskAsserter()
                .WithCommonBuildAgentsCheck();
            ret.pathResolver = new NUnitPathResolver();

            return ret;
        }

        /// <summary>
        /// Creates a new <see cref="DiskAsserter"/> and fully initialises
        /// </summary>
        /// <returns>A ready to use <see cref="DiskAsserter"/></returns>
        public static DiskAsserter InstanceWithRecommendedSettings(string solutionFolder, string outputFolder = "TestData")
        {
            var ret = new DiskAsserter()
                .WithRecommendedSettings(solutionFolder, outputFolder);

            ret.pathResolver = new NUnitPathResolver();

            return ret;
        }
    }
}
