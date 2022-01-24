using NUnit.Framework;

namespace MK94.Assert.NUnit.Test
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SetupDiskAssert.WithRecommendedSettings("MK94.Assert");

             DiskAssert.EnableWriteMode();
        }
    }
}
