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

        public void Begin()
        {
            if (diskAsserter.WriteMode)
                return;

            operations.Value = new Queue<AssertOperation>(diskAsserter.GetOperations().Where(x => x.Mode == OperationMode.Input));
        }

        public T Of<T>(Func<T> actual = default)
            where T : class
        {
            if (diskAsserter.WriteMode && actual != default)
                return proxyGenerator.CreateInterfaceProxyWithTarget(actual(), this);

            return proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(this);
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

            var stepPath = Path.Combine(diskAsserter.PathResolver.GetStepPath(), returnName + ".json");

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
            var expectedOperation = operations.Value.Dequeue();

            if (expectedOperation.Mode != OperationMode.Input)
                throw new InvalidOperationException($"Expecting input from {expectedOperation.Step} but actual is an output to {stepName}");

            var stepPath = Path.Combine(diskAsserter.PathResolver.GetStepPath(), stepName);

            if (expectedOperation.Step != stepPath)
                throw new InvalidOperationException($"Expecting input from {expectedOperation.Step} but actual is an input from {stepPath}");
        }
    }
}
