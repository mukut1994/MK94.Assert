using MK94.Assert.Mocking;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK94.Assert.NUnit.Test
{
    public class WithSetupTests
    {
        private IDatabase database = null!;

        [SetUp]
        public void Setup()
        {
            DiskAssert.Default
                .WithMocks()
                .Of<IDatabase>((m) => new Database(), out database);
        }

        [Test]
        public async Task InsertTest()
        {
            await database.Insert(1, "Text 1");
            await database.Insert(2, "Text 2");
            await database.Insert(3, "Text 3");

            DiskAssert.Matches("Step 1", await database.Select(3));
            DiskAssert.Matches("Step 2", await database.Select(1));
            DiskAssert.Matches("Step 3", await database.Select(2));

            DiskAssert.MatchesSequence();
        }

        [Test]
        public async Task SelectTest()
        {
            // TODO static withsetup
            await DiskAssert.Default.WithSetup(InsertTest);

            await database.Select(1);

            DiskAssert.Matches("Some manual check", 1);
            DiskAssert.MatchesSequence();
        }


        [Test]
        public async Task InsertMoreTest_NestedSetups()
        {
            DiskAssert.EnableWriteMode();

            await DiskAssert.Default.WithSetup(SelectTest);

            await database.Insert(4, "Text 4");

            DiskAssert.Matches("Some manual check", 2);
            DiskAssert.MatchesSequence();
        }
    }
}
