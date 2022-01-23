using MK94.Assert.Output;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MK94.Assert.NUnit
{
    public class NUnitPathResolver : IPathResolver
    {
        public string GetStepPath()
        {
            return Path.Combine(TestContext.CurrentContext.Test.ClassName, TestContext.CurrentContext.Test.Name);
        }
    }
}
