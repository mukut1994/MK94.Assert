using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MK94.Assert.NUnit.Test.CustomSettings
{
    public class Tests
    {
        [Test]
        public void SimpleTest()
        {
            DiskAssert.Matches(new { PropA = 1 }, "Step1");
        }

        [Test]
        public async Task AsyncTest()
        {
            await Task.Run(() => new { PropA = 1 }).Matches("Step1");
        }

        [Test]
        public async Task ExceptionTest()
        {
            await Task.Run(() => throw new ArgumentException("error"))
                .MatchesException<ArgumentException>("ExceptionStep");
        }

        [Test]
        [TestCase(1, 2)]
        [TestCase(2, 3)]
        public void StateMachineTest(int a, int b)
        {
            var stateMachine = new StateMachine();

            stateMachine.SetStateA(a);
            DiskAssert.Matches(stateMachine, "Setup A");

            stateMachine.SetStateB(b);
            DiskAssert.Matches(stateMachine, "Setup B");

            var result = stateMachine.Sum();
            DiskAssert.Matches(result, "Result");
        }
    }
}