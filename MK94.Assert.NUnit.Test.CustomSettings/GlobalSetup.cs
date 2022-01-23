using NUnit.Framework;

namespace MK94.Assert.NUnit.Test.CustomSettings
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SetupDiskAssert
                .WithRecommendedSettings("MK94.Assert", "TestData.CustomSettings");
            /*
                .WithBaseFolderRelativeToBinary<Configuration>("MK94.Assert", "TestData.CustomSettings")
                .WithFolderStructure(BasedOn.TestName)
                .WithPseudoRandom(BasedOn.TestName)
                .WithChecksumStructure(BasedOn.TestName)
                .WithDevModeOnEnvironmentVariable("CI", "true");
            */

            // DiskAssert.EnableWriteMode();
        }
    }
}
