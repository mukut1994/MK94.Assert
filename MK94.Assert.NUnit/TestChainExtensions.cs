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
        public static TestChainer From(this TestChainer chainer, Action step)
        {
            if (!step.Method.GetCustomAttributes(typeof(TestAttribute), false).Any())
                throw new InvalidOperationException($@"This does not seem to be a NUnit test method.
Call {nameof(TestChainer)}.{nameof(TestChainer.From)} instead to set it manually.");

            return chainer.From(step.Method.DeclaringType.FullName, step.Method.Name);
        }
    }
}
