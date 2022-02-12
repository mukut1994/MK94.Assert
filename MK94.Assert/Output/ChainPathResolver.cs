using System.IO;

namespace MK94.Assert.Output
{
    public class ChainPathResolver : IPathResolver
    {
        private readonly string className;
        private readonly string testCaseName;

        public ChainPathResolver(string className, string testCaseName)
        {
            this.className = className;
            this.testCaseName = testCaseName;
        }

        public string GetStepPath()
        {
            return Path.Combine(className, testCaseName);
        }
    }

}
