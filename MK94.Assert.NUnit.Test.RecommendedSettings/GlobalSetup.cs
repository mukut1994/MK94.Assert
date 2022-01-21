﻿using NUnit.Framework;

namespace MK94.Assert.NUnit.Test.RecommendedSettings
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SetupAssertConfiguration.WithRecommendedSettings<Configuration>();

            // AssertConfigure.EnableWriteMode();
        }
    }
}