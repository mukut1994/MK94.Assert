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
            if (parent.diskAsserter.InSetup)
            {
                invocation.ReturnValue = invocation.Method.Invoke(mocked.Value, invocation.Arguments);
                return;
            }

            parent.count ??= new Mocker.Counter();

            var stepName = $"{GetPathFriendlyClassName(invocation)}.{invocation.Method.Name}_{parent.count.Count}";
            var returnName = $"{stepName}_return.json";

            parent.count.Count++;

            MatchParameters(invocation, stepName);

            if (parent.diskAsserter.WriteMode)
            {
                invocation.ReturnValue = invocation.Method.Invoke(mocked.Value, invocation.Arguments);

                object toSerialize = UnwrapTaskReturnValue(invocation);

                var serialized = parent.diskAsserter.Serializer.Serialize(toSerialize);

                parent.diskAsserter.MatchesRaw(returnName, serialized, null, JsonDifferenceFormatter.Instance, OperationMode.Input);

                return;
            }

            EnsureExpectedOperationCalled(invocation, returnName);

            SetReturnValueFromPreviousRun(invocation, returnName);
        }

        private static string GetPathFriendlyClassName(IInvocation invocation)
        {
            return GetPathFriendlyClassName(invocation.Method.DeclaringType);
        }

        private static string GetPathFriendlyClassName(Type type)
        {
            if (type.IsGenericType)
            {
                var name = type.GetGenericTypeDefinition().FullName;
                var genericPart = type.GetGenericArguments().Select(g => GetPathFriendlyClassName(g)).Aggregate((a, b) => $"{a}, {b}");

                return $"{name.Substring(0, name.IndexOf('`'))}({genericPart})";
            }

            return type.FullName;
        }

        private void SetReturnValueFromPreviousRun(IInvocation invocation, string returnName)
        {
            var stepPath = Path.Combine(parent.diskAsserter.PathResolver.GetStepPath(), returnName).Replace('\\', '/');

            using var reader = parent.diskAsserter.Read(stepPath);

            if (MethodReturnIsVoid(invocation))
                return;

            if (MethodReturnIsTask(invocation))
            {
                invocation.ReturnValue = Task.FromResult(true);
                return;
            }

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

        private static object UnwrapTaskReturnValue(IInvocation invocation)
        {
            var ret = invocation.ReturnValue;

            if (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                ret = invocation.Method.ReturnType.GetProperty(nameof(Task<object>.Result)).GetValue(ret); // TODO should run task properly?

            else if (invocation.Method.ReturnType == typeof(Task))
            {
                // TODO should run task properly?
                var task = (Task)ret;

                if (!task.IsCompleted)
                    task.RunSynchronously();

                ret = null;
            }

            return ret;
        }

        private void MatchParameters(IInvocation invocation, string stepName)
        {
            var parameters = invocation.Method.GetParameters();

            for (var i = 0; i < invocation.Arguments.Length; i++)
            {
                parent.diskAsserter.Matches($"{stepName}_{parameters[i].Name}", invocation.Arguments[i]);
            }
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
            if (parent.operations == null)
                parent.operations = parent.diskAsserter.GetOperations();

            var expectedOperation = parent.operations.Skip(parent.diskAsserter.Operations.Count).First(x => x.Mode == OperationMode.Input);

            if (expectedOperation.Mode != OperationMode.Input)
                throw new InvalidOperationException($"Expecting input from {expectedOperation.Step} but actual is an output to {stepName}");

            var stepPath = Path.Combine(parent.diskAsserter.PathResolver.GetStepPath(), stepName).Replace('\\', '/');

            if (expectedOperation.Step != stepPath)
                throw new InvalidOperationException($"Expecting input from {expectedOperation.Step} but actual is an input from {stepPath}");
        }


    }
}
