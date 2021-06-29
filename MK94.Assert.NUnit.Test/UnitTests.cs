using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MK94.Assert.NUnit.Test
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
            await DiskAssert.Matches(Task.Run(() => new { PropA = 1 }), "Step1");
        }

        [Test]
        public async Task ExceptionTest()
        {
            await Task.Run(() => throw new ArgumentException("error"))
                .MatchesException<ArgumentException>("ExceptionStep");
        }

        [Test]
        public void StateMachineTest()
        {
            var stateMachine = new StateMachine();

            stateMachine.SetStateA(1);
            DiskAssert.Matches(stateMachine, "Setup A");

            stateMachine.SetStateB(2);
            DiskAssert.Matches(stateMachine, "Setup B");

            var result = stateMachine.Sum();
            DiskAssert.Matches(result, "Result");
        }
    }
}