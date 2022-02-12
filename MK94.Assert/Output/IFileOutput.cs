using System.IO;

namespace MK94.Assert.Output
{
	/// <summary>
	/// Used for writing/reading files. <br />
	/// E.g. write to disk/memory/network etc
	/// </summary>
    public interface IFileOutput
	{
		Stream OpenRead(string path);

		void Write(string path, Stream sourceStream);

		void Delete(string path);
	}
}
