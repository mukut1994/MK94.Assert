using MK94.Assert.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MK94.Assert.Chain
{
    public class TestChainer
    {
        public DiskAsserter DiskAsserter { get; }

        private List<ChainPathResolver> TestChainContexts { get; set; } = new List<ChainPathResolver>();

        public TestChainer(DiskAsserter diskAsserter)
        {
            DiskAsserter = diskAsserter;
        }

        public TestChainer From(string fullClassName, string testCaseName)
        {
            TestChainContexts.Add(new ChainPathResolver(fullClassName, testCaseName));

            return this;
        }
        
        public Stream OpenRead(string step)
        {
            // reverse order; get the latest context first
            for (var i = TestChainContexts.Count - 1; i > -1; i--)
            {
                var path = Path.Combine(TestChainContexts[i].GetStepPath(), step);

                var ret = DiskAsserter.Output.OpenRead(path, false);

                if (ret != null)
                {
                    DiskAsserter.Operations.Value = DiskAsserter.Operations.Value ?? new List<AssertOperation>();
                    DiskAsserter.Operations.Value.Add(new AssertOperation(OperationMode.Input, path));
                    return ret;
                }
            }

                throw new InvalidOperationException($@"The step {step} does not exist in any context.
Have you run the previous tests with EnableWriteMode or are missing a call to {nameof(From)}?");
        }

        public T Read<T>(string step)
        {
            using var stream = OpenRead(step);

            return DiskAsserter.Serializer.Deserialize<T>(stream);
        }

        public string Read(string step)
        {
            using var reader = new StreamReader(OpenRead(step));

            return reader.ReadToEnd();
        }
    }
}
