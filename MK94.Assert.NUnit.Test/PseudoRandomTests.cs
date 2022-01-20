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

                DiskAssert.Matches(randomizedObject, $"RandomizedObj_{i}");

                Thread.Sleep(1000);
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

                DiskAssert.Matches(randomizedObject, $"RandomizedObj_{i}");

                Thread.Sleep(1000);
            }
        }

        [Test]
        public void GuidTest()
        {
            DiskAssert.Matches(new PseudoRandomGuidProvider().NewGuid(), "Guid1");
            DiskAssert.Matches(new PseudoRandomGuidProvider().NewGuid(), "Guid2");
            DiskAssert.Matches(new PseudoRandomGuidProvider().NewGuid(), "Guid3");
        }
    }
}
