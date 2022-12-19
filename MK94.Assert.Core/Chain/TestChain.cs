using MK94.Assert.Output;
using System;
using System.Collections.Generic;
using System.IO;

namespace MK94.Assert.Input
{
    public class TestInput
    {
        private DiskAsserter DiskAsserter { get; }

        private List<IPathResolver> TestChainContexts { get; set; } = new List<IPathResolver>();

        public TestInput(DiskAsserter diskAsserter)
        {
            DiskAsserter = diskAsserter;
        }

        public TestInput FromPath(string path)
        {
            TestChainContexts.Add(new DirectPathResolver(path));

            return this;
        }

        public TestInput From(string fullClassName, string testCaseName)
        {
            TestChainContexts.Add(new ChainPathResolver(fullClassName, testCaseName));

            return this;
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
        
        private Stream OpenRead(string step)
        {
            // reverse order; get the latest context first
            for (var i = TestChainContexts.Count - 1; i > -1; i--)
            {
                var path = Path.Combine(TestChainContexts[i].GetStepPath(), step);

                var ret = DiskAsserter.Output.OpenRead(path, false);

                if (ret == null) continue;
                
                // Replace windows path \ with /
                DiskAsserter.Operations.Add(new AssertOperation(OperationMode.Input, path.Replace('\\', '/')));
                return ret;
            }

            throw new InvalidOperationException($@"The step {step} does not exist in any context.
Have you run the previous tests with EnableWriteMode or are missing a call to {nameof(From)}?");
        }
    }
}
