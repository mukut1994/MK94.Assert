using System.IO;

namespace MK94.Assert.Output
{
    public class DirectPathResolver : IPathResolver
    {
        private readonly string path;

        public DirectPathResolver(string path)
        {
            this.path = path;
        }

        public string GetStepPath()
        {
            return path;
        }
    }
}
