using MK94.Assert.Input;
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
        /// Adds a test to the context of this chain for any <see cref="TestInput.Read(string)"/> or related methods
        /// </summary>
        /// <param name="chainer">The chain to add context to</param>
        /// <param name="step">The NUnit method marked with [Test]</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static TestInput From(this TestInput chainer, Action step)
        {
            EnsureIsValidMethod(step);

            return chainer.From(step.Method.DeclaringType.FullName, step.Method.Name);
        }

        /// <summary>
        /// Adds a path to the context of this chain for any <see cref="TestInput.Read(string)"/> or related methods
        /// </summary>
        /// <param name="chainer">The chain to add context to</param>
        /// <param name="step">The path to add. <b>This will be relative to the binary location!</b>. For easier usage call <see cref="TestInput.FromPath(string, string)"/></param>
        /// <returns></returns>
        public static TestInput FromPath(this TestInput chainer, string path)
        {
            return chainer.FromPath(path);
        }

        /// <summary>
        /// Adds a path to the context of this chain for any <see cref="TestInput.Read(string)"/> or related methods.
        /// This should usually be the same as the call to <see cref="SetupDiskAssert.WithRecommendedSettings(string, string)"/>
        /// </summary>
        /// <param name="chainer">The chain to add context to</param>
        /// <param name="parentFolder">The parent folder to relative to the binary location</param>
        /// <param name="parentRelative">The test data folder to relative to the parent folder location</param>
        /// <returns></returns>
        public static TestInput FromPath(this TestInput chainer, string parentFolder, params string[] parentRelative)
        {
            return chainer.FromPath(parentFolder, System.IO.Path.Combine(parentRelative));
        }

        private static void EnsureIsValidMethod(Action step)
        {
            if (!step.Method.GetCustomAttributes(typeof(TestAttribute), false).Any())
                throw new InvalidOperationException($@"This does not seem to be a NUnit test method.
Call {nameof(TestInput)}.{nameof(TestInput.From)} instead to set it manually.");
        }

    }
}
