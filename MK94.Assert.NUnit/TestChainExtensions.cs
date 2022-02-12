using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MK94.Assert.NUnit
{
    public static class TestChainExtensions
    {
        public static TestChainer From(this TestChainer chainer, Action step)
        {
            if (!step.Method.GetCustomAttributes(typeof(Test), false).Any())
                throw new InvalidOperationException($@"This does not seem to be an NUnit test method.
Call {nameof(TestChainer.From)}(string, string) instead to set it manually.");

            return chainer.From(step.Method.DeclaringType.FullName, step.Method.Name);
        }
    }
}
