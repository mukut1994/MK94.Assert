using NUnit.Framework;
using System.Linq;

namespace MK94.Assert.NUnit.Test
{
    public class JsonDifferenceTests
    {
        [Test]
        public void DeduplicationMergesTwoFilesIntoOneTest()
        {
            var asserter = SetupDiskAssert.InstanceWithBasicSettings()
                .WithDevModeOnEnvironmentVariable("NONE", "NONE")
                .WithDeduplicationInMemory(out var output)
                .EnableWriteMode()
                .Build();

            var fileContent = 1;

            asserter.Matches("Basic Dedup in memory 1", fileContent);
            asserter.Matches("Basic Dedup in memory 2", fileContent);

            // On windows this causes root to contain \r\n 
            // On unix this is \n
            // Hacky fix make them both consistent
            output.Files["root.json"] = output.Files["root.json"].Replace("\r", string.Empty);

            DiskAssert.Matches("Deduplication in memory", output.Files.OrderBy(x => x.Key));
        }

        [Test]
        public void JsonDifferenceOnProperties()
        {
            var initialObject = new
            {
                PropA = 1,
                PropB = 2,
                PropC = new
                {
                    PropI = 1
                }
            };

            var updatedObject = new
            {
                PropA = 3,
                // PropB is deleted
                PropC = (object?) null
            };

            MatchDifferences(initialObject, updatedObject);
        }

        [Test]
        public void JsonDifferenceOnProperties_NullToValue()
        {
            var initialObject = new
            {
                PropA = 1,
                PropC = new
                {
                    PropI = new object()
                },
                PropD = new[]
                {
                    new {}
                }
            };

            var updatedObject = new
            {
                PropA = 3,
                PropB = 4,
                PropC = new
                {

                },
                PropD = new object[]
                {
                }
            };

            MatchDifferences(initialObject, updatedObject);
        }

        [Test]
        public void JsonDifferenceOnArray()
        {
            var initialObject = new []
            {
                1, 2, 3
            };

            var updatedObject = new []
            {
                1, 3
            };

            MatchDifferences(initialObject, updatedObject);
        }

        private void MatchDifferences(object initialObject, object updatedObject)
        {
            var asserter = SetupDiskAssert.InstanceWithBasicSettings()
                .WithDevModeOnEnvironmentVariable("NONE", "NONE")
                .WithDeduplicationInMemory(out _)
                .EnableWriteMode()
                .Build();

            asserter.Matches("Step 1", initialObject);
            asserter.DisableWriteMode();

            try
            {
                asserter.Matches("Step 1", updatedObject);
            }
            catch (DifferenceException ex)
            {
                DiskAssert.Matches("differences", ex.Differences);
            }
        }
    }
}
