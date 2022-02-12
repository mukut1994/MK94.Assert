using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK94.Assert.NUnit.Test
{
    public class ChainingTests
    {
        private class TestObject
        {
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
        }

        [Test]
        public void Step1()
        {
            DiskAssert.EnableWriteMode();

            DiskAssert.Matches("Step 1", new TestObject { A = 1, B = 2, C = 3});
        }

        [Test]
        public void Step2()
        {
            var x = new TestChainer();
            x.DiskAsserter = DiskAsserter.Default;

            x.GetContextOf("MK94.Assert.NUnit.Test." + nameof(ChainingTests), nameof(Step1));

            var ret = x.Read<TestObject>("Step 1.json");
        }
    }
}
