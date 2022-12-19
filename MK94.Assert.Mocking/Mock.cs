using System;
using System.Collections.Generic;
using System.Text;

namespace MK94.Assert.Mocking
{
    public static class Mock
    {
        /// <inheritdoc cref="Mocker.Default"/>
        public static Mocker Default
        {
            get
            {
                Mocker.Default.Value ??= DiskAssert.Default.WithMocks();
                return Mocker.Default.Value;
            }
        }

        /// <inheritdoc cref="Mocker.SetContext(object)"/>
        public static Mocker SetContext(object context) => Default.SetContext(context);

        /// <inheritdoc cref="Mocker.Of{T}(Func{MockContext, T})"/>
        public static T Of<T>(Func<MockContext, T> actual) where T : class => Default.Of(actual);

        /// <inheritdoc cref="Mocker.Of{T}(Func{MockContext, T}, out T)" />
        public static Mocker Of<T>(Func<MockContext, T> actual, out T mocked) where T : class => Default.Of(actual, out mocked);

        /// <inheritdoc cref="Mocker.Of(Type, Func{MockContext, object})"/>
        public static object Of(Type type, Func<MockContext, object> actual) => Default.Of(type, actual);

        /// <inheritdoc cref="Mocker.Of(Type, Func{MockContext, object}, out object)"/>
        public static Mocker Of(Type type, Func<MockContext, object> actual, out object mocked) => Default.Of(type, actual, out mocked);
    }
}
