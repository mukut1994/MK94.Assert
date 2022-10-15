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
            parent.count.Value = parent.count.Value ?? new Mocker.Counter();

            var stepName = $"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}_{parent.count.Value.Count}";
            var returnName = $"{stepName}_return";
            parent.count.Value.Count++;

            var parameters = invocation.Method.GetParameters();

            string ParamName(int i) => $"{stepName}_{parameters[i].Name}";

            for (var i = 0; i < invocation.Arguments.Length; i++)
            {
                parent.diskAsserter.Matches($"{stepName}_{ParamName(i)}", invocation.Arguments[i]);
            }

            if (parent.diskAsserter.WriteMode)
            {
                invocation.ReturnValue = invocation.Method.Invoke(mocked.Value, invocation.Arguments);

                var toSerialize = invocation.ReturnValue;

                if (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    toSerialize = invocation.Method.ReturnType.GetMember(nameof(Task<object>.Result)); // TODO should run task properly?

                var serialized = parent.diskAsserter.Serializer.Serialize(toSerialize);

                parent.diskAsserter.MatchesRaw(returnName, serialized, "json", JsonDifferenceFormatter.Instance, OperationMode.Input);

                return;
            }

            EnsureExpectedOperationCalled(invocation, returnName + ".json");

            var stepPath = Path.Combine(parent.diskAsserter.PathResolver.GetStepPath(), returnName + ".json").Replace('\\', '/');

            parent.diskAsserter.Operations.Value.Add(new AssertOperation(OperationMode.Input, stepPath));

            if (MethodReturnIsVoid(invocation))
                return;

            if (MethodReturnIsTask(invocation))
            {
                invocation.ReturnValue = Task.FromResult(true);
                return;
            }

            using var reader = parent.diskAsserter.Output.OpenRead(stepPath, false);

            if (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var result = parent.diskAsserter.Serializer
                .GetType()
                .GetMethod(nameof(ISerializer.Deserialize))
                .MakeGenericMethod(invocation.Method.ReturnType.GetGenericArguments()[0])
                .Invoke(parent.diskAsserter.Serializer, new object[] { reader });

                invocation.ReturnValue = typeof(Task)
                    .GetMethod(nameof(Task.FromResult), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .MakeGenericMethod(invocation.Method.ReturnType.GetGenericArguments()[0])
                    .Invoke(null, new[] { result });

                return;
            }

                invocation.ReturnValue = parent.diskAsserter.Serializer
                .GetType()
                .GetMethod(nameof(ISerializer.Deserialize))
                .MakeGenericMethod(invocation.Method.ReturnType)
                .Invoke(parent.diskAsserter.Serializer, new object[] { reader });

        }

        private bool MethodReturnIsVoid(IInvocation invocation)
        {
            return invocation.Method.ReturnType == typeof(void);
        }

        private bool MethodReturnIsTask(IInvocation invocation)
        {
            return invocation.Method.ReturnType == typeof(Task);
        }

        private void EnsureExpectedOperationCalled(IInvocation invocation, string stepName)
        {
            if (parent.operations.Value == null)
                parent.operations.Value = parent.diskAsserter.GetOperations();

            var expectedOperation = parent.operations.Value.Skip(parent.diskAsserter.Operations.Value.Count).First(x => x.Mode == OperationMode.Input);

            if (expectedOperation.Mode != OperationMode.Input)
                throw new InvalidOperationException($"Expecting input from {expectedOperation.Step} but actual is an output to {stepName}");

            var stepPath = Path.Combine(parent.diskAsserter.PathResolver.GetStepPath(), stepName).Replace('\\', '/');

            if (expectedOperation.Step != stepPath)
                throw new InvalidOperationException($"Expecting input from {expectedOperation.Step} but actual is an input from {stepPath}");
        }
    }
}
