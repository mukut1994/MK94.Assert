using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MK94.Assert.NUnit.Test
{
    /// <summary>
    /// Chain tests where values from any <see cref="DiskAssert.WithSetup"/> calls is preferred.
    /// 
    /// This is useful for scenarios where a setup needs to insert some pseudo random values
    /// and the current test wants to make use of those values
    /// 
    /// Without preferring the <see cref="DiskAssert.WithSetup"/> the Ids would end up being different
    /// because the pseudo randomizer is based on the <b>current</b> test
    /// </summary>
    public class ChainingTestsWithSetup
    {
        private class TestObject
        {
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
        }

        [Test]
        public void PreventChainingMethodNonNUnit()
        {
            // TODO add non-task overload to MatchesException<T>
            DiskAssert.MatchesException<InvalidOperationException>("Exception non test method",
                Task.Run(() => DiskAssert.WithInputs().From(() => { }))); 
        }

        [Test]
        public void TestCase1()
        {
            DiskAssert.Matches("Step 1", new TestObject
            {
                A = PseudoRandom.Int(),
                B = PseudoRandom.Int(),
                C = PseudoRandom.Int(),
            });
        }

        [Test]
        public void TestCase2()
        {
            DiskAssert.EnableWriteMode();

            var inputs = DiskAssert
                .WithInputs()
                .From(TestCase1);

            // Should never be called before all WithSetup calls
            var context = inputs.Read<TestObject>("Step 1");

            DiskAssert.WithSetup(TestCase1);

            // Should now be different
            // TestCase 1 
            var contextAfter = inputs.Read<TestObject>("Step 1");

            var diff = new JsonDifferenceFormatter().FindDifferences(
                JsonSerializer.Serialize(context),
                JsonSerializer.Serialize(contextAfter));

            DiskAssert.Matches("Step 1 before after diff", diff);
            DiskAssert.MatchesSequence();
        }

        [Test]
        [Ignore("Don't run so we can test case 4 with input from setup only")]
        public void TestCase3()
        {
            DiskAssert.Matches("Step 3", new TestObject
            {
                A = PseudoRandom.Int(),
                B = PseudoRandom.Int(),
                C = PseudoRandom.Int(),
            });
        }

        [Test]
        public void TestCase4()
        {
            DiskAssert.EnableWriteMode();

            var inputs = DiskAssert
                .WithInputs()
                .From(TestCase3);

            DiskAssert.WithSetup(TestCase3);

            // Should be read from the setup and not disk
            var contextAfter = inputs.Read<TestObject>("Step 3");

            DiskAssert.Matches("Step 3", contextAfter);
            DiskAssert.MatchesSequence();
        }
    }
}
