using NUnit.Framework;

namespace MK94.Assert.NUnit.Test.RecommendedSettings
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SetupDiskAssert
                .WithRecommendedSettings("MK94.Assert", "TestData.RecommendedSettings");

            // DiskAssert.EnableWriteMode();
        }
    }
}
