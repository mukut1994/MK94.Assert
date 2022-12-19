using Castle.DynamicProxy;
using MK94.Assert.Input;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MK94.Assert.Mocking
{
    /// <summary>
    /// A class to generate mocks that expect calls from <see cref="DiskAsserter.Operations"/> and setup returns via <see cref="DiskAsserter.Matches{T}(string, T)"/>
    /// </summary>
    public class Mocker
    {
        // somewhat hacky workaround 
        // the counter value is reset between threads unless it's a ref type
        internal class Counter
        {
            public int Count { get; set; }
        }

        /// <summary>
        /// The default Mocker based on <see cref="DiskAssert.Default"/> <br />
        /// Used by the static methods in <see cref="Mock"/>
        /// </summary>
        public static AsyncLocal<Mocker> Default { get; } = new AsyncLocal<Mocker>();

        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        internal readonly DiskAsserter diskAsserter;

        internal List<AssertOperation> operations;
        internal Counter count = new Counter();

        internal MockContext instanceResolveContext;

        public Mocker(DiskAsserter diskAsserter)
        {
            this.diskAsserter = diskAsserter;

            instanceResolveContext = new MockContext(this, diskAsserter);
        }

        /// <summary>
        /// Sets the property <see cref="MockContext.CustomContext"/> of the context passed to the instance resolver lambdas <br />
        /// Useful for creating instances that rely on other instances that are initialized late but still need to be registered with a DI Container e.g. IServiceCollection and IServiceProvider
        /// </summary>
        /// <param name="context">The value that will be passed through</param>
        /// <returns>The mocker this method was called on</returns>
        public Mocker SetContext(object context)
        {
            instanceResolveContext.CustomContext = context;

            return this;
        }

        /// <summary>
        /// Creates a mock of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The class or interface to mock</typeparam>
        /// <param name="actual">The implementation to use when running in write mode. <br />
        /// The calls and results of this implementation will be record for future re-runs.</param>
        /// <returns>A mocked instance of <typeparamref name="T"/></returns>
        public T Of<T>(Func<MockContext, T> actual)
            where T : class
        {
            return ProxyGenerator.CreateInterfaceProxyWithoutTarget<T>(new Interceptor(this, actual));
        }

        /// <inheritdoc cref="Of{T}(Func{MockContext, T})"/>
        public object Of(Type type, Func<MockContext, object> actual) 
        {
            return ProxyGenerator.CreateInterfaceProxyWithoutTarget(type, new Interceptor(this, actual));
        }

        /// <inheritdoc cref="Of{T}(Func{MockContext, T}, out T)"/>
        public Mocker Of(Type type, Func<MockContext, object> actual, out object mocked)
        {
            mocked = ProxyGenerator.CreateInterfaceProxyWithoutTarget(type, new Interceptor(this, actual));

            return this;
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
        public Mocker Of<T>(Func<MockContext, T> actual, out T mocked)
            where T : class
        {
            mocked = Of(actual);

            return this;
        }
    }
}
