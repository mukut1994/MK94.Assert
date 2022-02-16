using MK94.Assert.Chain;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MK94.Assert.NUnit
{
    public static class TestChainExtensions
    {
        /// <summary>
        /// Adds a test to the context of this chain for any <see cref="TestChainer.Read(string)"/> or related methods
        /// </summary>
        /// <param name="chainer">The chain to add context to</param>
        /// <param name="step">The NUnit method marked with [Test]</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static TestChainer From(this TestChainer chainer, Action step)
        {
            EnsureIsValidMethod(step);

            return chainer.From(step.Method.DeclaringType.FullName, step.Method.Name);
        }

        private static void EnsureIsValidMethod(Action step)
        {
            if (!step.Method.GetCustomAttributes(typeof(TestAttribute), false).Any())
                throw new InvalidOperationException($@"This does not seem to be a NUnit test method.
Call {nameof(TestChainer)}.{nameof(TestChainer.From)} instead to set it manually.");
        }

    }
}
