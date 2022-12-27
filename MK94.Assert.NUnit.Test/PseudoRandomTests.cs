using NUnit.Framework;
using System.Threading;

namespace MK94.Assert.NUnit.Test
{
    [Parallelizable(ParallelScope.All)]
    public class PseudoRandomTests
    {
        [Test]
        public void RandomizerTest()
        {
            // Test needs to run in parallel to RandomizerTest2
            // Checks that Parallel test runs don't share randomizer seeds

            for (int i = 0; i < 10; i++)
            {
                var randomizedObject = new
                {
                    String = PseudoRandom.String(),
                    Int = PseudoRandom.Int(),
                    DateTime = PseudoRandom.DateTime()
                };

                DiskAssert.Matches($"RandomizedObj_{i}", randomizedObject);

                Thread.Sleep(500);
            }
        }

        [Test]
        public void RandomizerTest2()
        {
            // Test needs to run in parallel to RandomizerTest
            // Checks that Parallel test runs don't share randomizer seeds

            for (int i = 0; i < 10; i++)
            {
                var randomizedObject = new
                {
                    String = PseudoRandom.String(),
                    Int = PseudoRandom.Int(),
                    DateTime = PseudoRandom.DateTime()
                };

                DiskAssert.Matches($"RandomizedObj_{i}", randomizedObject);

                Thread.Sleep(500);
            }
        }

        [Test]
        public void GuidTest()
        {
            DiskAssert.Matches("Guid1", new PseudoRandomGuidProvider().NewGuid());
            DiskAssert.Matches("Guid2", new PseudoRandomGuidProvider().NewGuid());
            DiskAssert.Matches("Guid3", new PseudoRandomGuidProvider().NewGuid());
        }

        [Test]
        public void GuidTestWithChain()
        {
            DiskAssert.WithSetup(GuidTest);

            DiskAssert.Matches("Guid4", new PseudoRandomGuidProvider().NewGuid());
            DiskAssert.Matches("Guid5", new PseudoRandomGuidProvider().NewGuid());
            DiskAssert.Matches("Guid6", new PseudoRandomGuidProvider().NewGuid());
        }
    }
}
