
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MK94.Assert.NUnit.Test
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            AssertConfigureHelper
                .WithBaseFolderRelativeToBinary("MK94.Assert", "TestData")
                .WithFolderStructure(BasedOn.TestName)
                .WithPseudoRandom(BasedOn.TestName)
                .WithChecksumStructure(BasedOn.TestName)
                .WithDevModeOnEnvironmentVariable("CI", "true");

            AssertConfigure.EnableWriteMode();
        }

    }
}
