﻿using NUnit.Framework;

namespace MK94.Assert.NUnit.Test
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SetupAssertConfiguration
                .WithBaseFolderRelativeToBinary<Configuration>("MK94.Assert", "TestData")
                .WithFolderStructure(BasedOn.TestName)
                .WithPseudoRandom(BasedOn.TestName)
                .WithChecksumStructure(BasedOn.TestName)
                .WithDevModeOnEnvironmentVariable("CI", "true");

            // AssertConfigure.EnableWriteMode();
        }

    }
}
