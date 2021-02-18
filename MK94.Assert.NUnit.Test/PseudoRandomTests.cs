using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MK94.Assert.NUnit.Test
{
    [Parallelizable(ParallelScope.All)]
    public class PseudoRandomTests
    {
        [Test]
        public void RandomizerTest()
        {
            for (int i = 0; i < 10; i++)
            {
                var randomizedObject = new
                {
                    String = PseudoRandom.String(),
                    Int = PseudoRandom.Int(),
                    DateTime = PseudoRandom.DateTime()
                };

                Assert.Matches(randomizedObject, $"RandomizedObj_{i}");

                Thread.Sleep(1000);
            }
        }

        [Test]
        public void RandomizerTest2()
        {
            for (int i = 0; i < 10; i++)
            {
                var randomizedObject = new
                {
                    String = PseudoRandom.String(),
                    Int = PseudoRandom.Int(),
                    DateTime = PseudoRandom.DateTime()
                };

                Assert.Matches(randomizedObject, $"RandomizedObj_{i}");

                Thread.Sleep(1000);
            }
        }

        [Test]
        public void GuidTest()
        {
            Assert.Matches(new PseudoRandomGuidProvider().NewGuid(), "Guid1");
            Assert.Matches(new PseudoRandomGuidProvider().NewGuid(), "Guid2");
            Assert.Matches(new PseudoRandomGuidProvider().NewGuid(), "Guid3");
        }
    }
}
