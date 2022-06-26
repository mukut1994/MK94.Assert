using System;
using System.Collections.Generic;
using System.Text;

namespace MK94.Assert.Mocking
{
    public class MockContext
    {
        public Mocker Mocker { get; }

        public DiskAsserter Asserter { get; }

        public object CustomContext { get; internal set; }

        public MockContext(Mocker mocker, DiskAsserter asserter)
        {
            Mocker = mocker;
            Asserter = asserter;
        }
    }
}
