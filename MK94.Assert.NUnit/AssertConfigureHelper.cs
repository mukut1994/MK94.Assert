using NUnit.Framework;
using System;
using System.IO;

namespace MK94.Assert.NUnit
{
    public static class AssertConfigureHelper
    {
        /// <inheritdoc cref="AssertConfigure.PathRelativeToParentFolder"/>
        public static Configure WithBaseFolderRelativeToBinary(string relativeParentFolder, string parentRelativeChild)
        {
            AssertConfigure.GlobalPath = AssertConfigure.PathRelativeToParentFolder(relativeParentFolder, parentRelativeChild);

            return new Configure();
        }

        public static Configure WithFolderStructure(this Configure c, BasedOn basedOn)
        {
            AssertConfigure.PathResolver = x => BasedOnPath(basedOn);

            return c;
        }

        public static Configure WithPseudoRandom(this Configure c, BasedOn basedOn)
        {
            PseudoRandom.WithBaseSeed(() => BasedOnPath(basedOn));

            return c;
        }

        public static Configure WithDevModeOnEnvironmentVariable(this Configure c, string environmentVariable, string valueOnProd)
        {
            AssertConfigure.IsDevEnvironment = Environment.GetEnvironmentVariable(environmentVariable) != valueOnProd;

            return c;
        }

        private static string BasedOnPath(BasedOn basedOn)
        {
            if (basedOn == BasedOn.TestName)
                return TestContext.CurrentContext.Test.MethodName;
            else if (basedOn == BasedOn.ClassNameTestName)
                return Path.Combine(TestContext.CurrentContext.Test.ClassName, TestContext.CurrentContext.Test.MethodName);

            throw new NotImplementedException(basedOn.ToString());
        }
    }

    public enum BasedOn
    {
        TestName,
        ClassNameTestName
    }

    /// <summary>
    /// Helper class to just make <see cref="AssertConfigureHelper"/> a builder like class
    /// </summary>
    public class Configure
    {
        internal Configure() { }
    }
}
