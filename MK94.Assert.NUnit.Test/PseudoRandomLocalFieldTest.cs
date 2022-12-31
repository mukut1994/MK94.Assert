using MK94.Assert.Mocking;
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
        private IDatabase database;

        [SetUp]
        public void Setup()
        {
            database = Mock.Of<IDatabase>(x => new Database());
        }

        [Test]
        public async Task TestStep1()
        {
            await database.Insert(1, DiskAssert.Default.PseudoRandomizer.String());

            DiskAssert.MatchesSequence();
        }

        [Test]
        public async Task TestStep2()
        {
            await DiskAssert.WithSetup(TestStep1);

            await database.Insert(2, DiskAssert.Default.PseudoRandomizer.String());

            DiskAssert.MatchesSequence();
        }

        [Test]
        public async Task TestStep3()
        {
            await DiskAssert.WithSetup(TestStep2);

            await database.Insert(3, DiskAssert.Default.PseudoRandomizer.String());

            DiskAssert.Matches("DB should have 3 unique values", await database.Everything());

            DiskAssert.MatchesSequence();
        }
    }
}
