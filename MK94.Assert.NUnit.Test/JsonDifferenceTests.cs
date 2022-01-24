using NUnit.Framework;
using System.Linq;

namespace MK94.Assert.NUnit.Test
{
    public class JsonDifferenceTests
    {
        [Test]
        public void DeduplicationMergesTwoFilesIntoOneTest()
        {
            var asserter = SetupDiskAssert.InstanceWithBasicSettings().WithDeduplicationInMemory(out var output).EnableWriteMode();

            var fileContent = 1;

            asserter.Matches("Basic Dedup in memory 1", fileContent);
            asserter.Matches("Basic Dedup in memory 2", fileContent);

            // On windows this causes root to contain \r\n 
            // On unix this is \n
            // Hacky fix make them both consistent
            output.files["root.json"] = output.files["root.json"].Replace("\r", string.Empty);

            DiskAssert.Matches("Deduplication in memory", output.files.OrderBy(x => x.Key));
        }

        [Test]
        public void JsonDifferenceOnProperties()
        {
            var initialObject = new
            {
                PropA = 1,
                PropB = 2
            };

            var updatedObject = new
            {
                PropA = 3,
                // PropB is deleted
            };

            MatchDifferences(initialObject, updatedObject);
        }

        [Test]
        public void JsonDifferenceOnArray()
        {
            var initialObject = new int[]
            {
                1, 2, 3
            };

            var updatedObject = new int[]
            {
                1, 3
            };

            MatchDifferences(initialObject, updatedObject);
        }

        private void MatchDifferences(object initialObject, object updatedObject)
        {
            var asserter = SetupDiskAssert.InstanceWithBasicSettings().WithDeduplicationInMemory(out _).EnableWriteMode();

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
