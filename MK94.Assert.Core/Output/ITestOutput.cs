using System.IO;

namespace MK94.Assert.Output
{
	/// <summary>
	/// Used for writing and reading Test output data. <br />
	/// Responsible for caching reads, physical file location and hashing.
	/// </summary>
	public interface ITestOutput
    {
        string GetAbsolutePathOf(string path);

        Stream OpenRead(string path, bool cache);

		bool IsHashMatch(string path, string rawData);

		void Write(string path, string rawData);
	}
}
