using Castle.DynamicProxy;
using MK94.Assert.Chain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MK94.Assert.Mocking
{
    /// <summary>
    /// A class to generate mocks that expect calls from <see cref="DiskAsserter.Operations"/> and setup returns via <see cref="DiskAsserter.Matches{T}(string, T)"/>
    /// </summary>
    public class Mocker : IInterceptor
    {
        private static readonly ProxyGenerator proxyGenerator = new ProxyGenerator();

        private readonly DiskAsserter diskAsserter;

        private AsyncLocal<Queue<AssertOperation>> operations = new AsyncLocal<Queue<AssertOperation>>();
        private AsyncLocal<int> count = new AsyncLocal<int>();

        public Mocker(DiskAsserter diskAsserter)
        {
            this.diskAsserter = diskAsserter;
        }

        /// <summary>
        /// Creates a mock of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The class or interface to mock</typeparam>
        /// <param name="actual">The implementation to use when running in write mode. <br />
        /// The calls and results of this implementation will be record for future re-runs.</param>
        /// <returns>A mocked instance of <typeparamref name="T"/></returns>
        public T Of<T>(Func<T> actual)
            where T : class
        {
            if (diskAsserter.WriteMode && actual != default)
                return proxyGenerator.CreateInterfaceProxyWithTarget(actual(), this);

            return proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(this);
        }


        /// <summary>
        /// Creates a mock of <typeparamref name="T"/>. Useful for a builder pattern.
        /// </summary>
        /// <typeparam name="T">The class or interface to mock</typeparam>
        /// <param name="actual">The implementation to use when running in write mode. <br />
        /// The calls and results of this implementation will be record for future re-runs.</param>
        /// <returns>A mocked instance of <typeparamref name="T"/></returns>
        /// <param name="mocked">The result mock object</param>
        /// 
        /// <example>
            /// <code>
                /// 
                /// DiskAssert.Default
                ///     .Of&lt;T1&gt;(() => new ImplementationOfT(), out var mocked1)
                ///     .Of&lt;T2&gt;(() => new ImplementationOfT2(), out var mocked2)
                ///     
            /// </code>
        /// </example>
        public Mocker Of<T>(Func<T> actual, out T mocked)
            where T : class
        {
            mocked = Of(actual);

            return this;
        }

        void IInterceptor.Intercept(IInvocation invocation)
        {
            var stepName = $"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}_{count.Value}";
            var returnName = $"{stepName}_return";
            count.Value++;

            var parameters = invocation.Method.GetParameters();

            string paramName(int i) => $"{stepName}_{parameters[i].Name}";

            for (var i = 0; i < invocation.Arguments.Length; i++)
            {
                diskAsserter.Matches($"{stepName}_{paramName(i)}", invocation.Arguments[i]);
            }

            if (diskAsserter.WriteMode)
            {
                invocation.Proceed();

                var serialized = diskAsserter.Serializer.Serialize(invocation.ReturnValue);
                
                diskAsserter.MatchesRaw(returnName, serialized, "json", JsonDifferenceFormatter.Instance, OperationMode.Input);

                return;
            }

            EnsureExpectedOperationCalled(invocation, returnName + ".json");

            var stepPath = Path.Combine(diskAsserter.PathResolver.GetStepPath(), returnName + ".json").Replace('\\', '/');

            diskAsserter.Operations.Value.Add(new AssertOperation(OperationMode.Input, stepPath));

            if (!MethodReturnIsVoid(invocation))
            {
                using var reader = diskAsserter.Output.OpenRead(stepPath, false);

                invocation.ReturnValue = diskAsserter.Serializer
                    .GetType()
                    .GetMethod(nameof(ISerializer.Deserialize))
                    .MakeGenericMethod(invocation.Method.ReturnType)
                    .Invoke(diskAsserter.Serializer, new object[] { reader });
            }
        }

        private bool MethodReturnIsVoid(IInvocation invocation)
        {
            return invocation.Method.ReturnType == typeof(void) || invocation.Method.ReturnType == typeof(Task);
        } 

        private void EnsureExpectedOperationCalled(IInvocation invocation, string stepName)
        {
            if(operations.Value == null)
                operations.Value = new Queue<AssertOperation>(diskAsserter.GetOperations().Where(x => x.Mode == OperationMode.Input));

            var expectedOperation = operations.Value.Dequeue();

            if (expectedOperation.Mode != OperationMode.Input)
                throw new InvalidOperationException($"Expecting input from {expectedOperation.Step} but actual is an output to {stepName}");

            var stepPath = Path.Combine(diskAsserter.PathResolver.GetStepPath(), stepName);

            if (expectedOperation.Step != stepPath)
                throw new InvalidOperationException($"Expecting input from {expectedOperation.Step} but actual is an input from {stepPath}");
        }
    }
}
