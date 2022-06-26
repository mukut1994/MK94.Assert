using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MK94.Assert.Mocking
{
    internal class Interceptor : IInterceptor
    {
        private Mocker parent;
        private Lazy<object> mocked;

        public Interceptor(Mocker parent, Func<MockContext, object> actual)
        {
            this.parent = parent;

            mocked = new Lazy<object>(() => actual(parent.instanceResolveContext));
        }

        void IInterceptor.Intercept(IInvocation invocation)
        {
            var stepName = $"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}_{parent.count.Value}";
            var returnName = $"{stepName}_return";
            parent.count.Value++;

            var parameters = invocation.Method.GetParameters();

            string ParamName(int i) => $"{stepName}_{parameters[i].Name}";

            for (var i = 0; i < invocation.Arguments.Length; i++)
            {
                parent.diskAsserter.Matches($"{stepName}_{ParamName(i)}", invocation.Arguments[i]);
            }

            if (parent.diskAsserter.WriteMode)
            {
                invocation.Method.Invoke(mocked.Value, invocation.Arguments);

                var serialized = parent.diskAsserter.Serializer.Serialize(invocation.ReturnValue);

                parent.diskAsserter.MatchesRaw(returnName, serialized, "json", JsonDifferenceFormatter.Instance, OperationMode.Input);

                return;
            }

            EnsureExpectedOperationCalled(invocation, returnName + ".json");

            var stepPath = Path.Combine(parent.diskAsserter.PathResolver.GetStepPath(), returnName + ".json").Replace('\\', '/');

            parent.diskAsserter.Operations.Value.Add(new AssertOperation(OperationMode.Input, stepPath));

            if (MethodReturnIsVoid(invocation))
                return;

            using var reader = parent.diskAsserter.Output.OpenRead(stepPath, false);

            invocation.ReturnValue = parent.diskAsserter.Serializer
                .GetType()
                .GetMethod(nameof(ISerializer.Deserialize))
                .MakeGenericMethod(invocation.Method.ReturnType)
                .Invoke(parent.diskAsserter.Serializer, new object[] { reader });

        }

        private bool MethodReturnIsVoid(IInvocation invocation)
        {
            return invocation.Method.ReturnType == typeof(void) || invocation.Method.ReturnType == typeof(Task);
        }

        private void EnsureExpectedOperationCalled(IInvocation invocation, string stepName)
        {
            if (parent.operations.Value == null)
                parent.operations.Value = new Queue<AssertOperation>(parent.diskAsserter.GetOperations().Where(x => x.Mode == OperationMode.Input));

            var expectedOperation = parent.operations.Value.Dequeue();

            if (expectedOperation.Mode != OperationMode.Input)
                throw new InvalidOperationException($"Expecting input from {expectedOperation.Step} but actual is an output to {stepName}");

            var stepPath = Path.Combine(parent.diskAsserter.PathResolver.GetStepPath(), stepName).Replace('\\', '/');

            if (expectedOperation.Step != stepPath)
                throw new InvalidOperationException($"Expecting input from {expectedOperation.Step} but actual is an input from {stepPath}");
        }
    }
}
