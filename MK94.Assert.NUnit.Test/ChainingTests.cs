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
        public void PreventChainingMethodNonNUnit()
        {
            // TODO add non-task overload to MatchesException<T>
            DiskAssert.MatchesException<InvalidOperationException>("Exception non test method",
                Task.Run(() => DiskAssert.WithInputs().From(() => { }))); 
        }

        [Test]
        public void TestCase1()
        {
            DiskAssert.Matches("Step 1", new TestObject { A = 1, B = 2, C = 3 });
        }

        [Test]
        public void TestCase2()
        {
            var inputs = DiskAssert
                .WithInputs()
                .From(TestCase1);

            var context = inputs.Read<TestObject>("Step 1.json");

            context.A += 10;
            context.B += 10;
            context.C += 10;

            DiskAssert.Matches("Step 1", context);
        }

        [Test]
        public void TestCase3()
        {
            var inputs = DiskAssert
                .WithInputs()
                .From(TestCase1)
                .From(TestCase2);

            var context = inputs.Read<TestObject>("Step 1.json");

            context.A += 10;
            context.B += 10;
            context.C += 10;

            DiskAssert.Matches("Step 1", context);
        }
    }
}
