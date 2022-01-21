using NUnit.Framework;
using System;
using System.IO;

namespace MK94.Assert.NUnit
{
    /// <summary>
    /// Helper class to just make <see cref="SetupAssertConfiguration"/> a builder like class
    /// </summary>
    public class Configuration : IConfiguration
    {
        public IConfiguration WithFolderStructure(BasedOn basedOn)
        {
            AssertConfigure.AddPathResolver(x => BasedOnPath(basedOn));

            return this;
        }

        public IConfiguration WithChecksumStructure(BasedOn basedOn)
        {
            AssertConfigure.AddChecksumFileResolver(x => Path.Combine(BasedOnPath(basedOn), "checksum"));

            return this;
        }

        public IConfiguration WithPseudoRandom(BasedOn basedOn)
        {
            PseudoRandom.WithBaseSeed(() => BasedOnPath(basedOn));

            return this;
        }

        public IConfiguration WithDevModeOnEnvironmentVariable(string environmentVariable, string valueOnProd)
        {
            AssertConfigure.IsDevEnvironment = Environment.GetEnvironmentVariable(environmentVariable) != valueOnProd;

            return this;
        }

        private static string BasedOnPath(BasedOn basedOn)
        {
            return basedOn switch
            {
                BasedOn.TestName => TestContext.CurrentContext.Test.FullName,
                BasedOn.ClassNameTestName => Path.Combine(TestContext.CurrentContext.Test.ClassName, TestContext.CurrentContext.Test.FullName),
                _ => throw new NotImplementedException(basedOn.ToString())
            };
        }
    }
}
