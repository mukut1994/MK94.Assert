using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK94.Assert.NUnit.Test
{
    public class PseudoRandomWithSetupStateTest
    {
        private List<string> someDbState;

        [SetUp]
        public void Setup()
        {
            someDbState = new List<string>();            
        }

        [Test]
        public void TestStep1()
        {
            someDbState.Add(DiskAssert.Default.PseudoRandomizer.String());
        }

        [Test]
        public void TestStep2()
        {
            DiskAssert.WithSetup(TestStep1);

            someDbState.Add(DiskAssert.Default.PseudoRandomizer.String());
        }

        [Test]
        public void TestStep3()
        {
            DiskAssert.WithSetup(TestStep2);

            someDbState.Add(DiskAssert.Default.PseudoRandomizer.String());

            DiskAssert.Matches("DB should have 3 unique values", someDbState);
        }
    }
}
