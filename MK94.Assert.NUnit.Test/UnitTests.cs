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
            Assert.Matches(new { PropA = 1 }, "Step1");
        }

        [Test]
        public async Task AsyncTest()
        {
            await Assert.Matches(Task.Run(() => new { PropA = 1 }), "Step1");
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
            Assert.Matches(stateMachine, "Setup A");

            stateMachine.SetStateB(2);
            Assert.Matches(stateMachine, "Setup B");

            var result = stateMachine.Sum();
            Assert.Matches(result, "Result");
        }
    }
}