using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK94.Assert.NUnit.Test
{
    public class OutputTests
    {
        [Test]
        public void WriteToDiskTest()
        {
            var path = PathHelper.PathRelativeToParentFolder("MK94.Assert", "DiskTests");
            var output = new Output.DiskFileOutput(path);

            output.Write("File 1", StringToStream("Hello world"));
            output.Write("File 2", StringToStream("Hello world 2"));

            var readBack = new StreamReader(output.OpenRead("File 1")).ReadToEnd();

            DiskAssert.Matches("File 1 Content", readBack);

            DiskAssert.Matches("Directory Structure", Directory.GetFiles(path).Select(Path.GetFileName));

            output.Delete("File 2");

            DiskAssert.Matches("Directory Structure after delete", Directory.GetFiles(path).Select(Path.GetFileName));
        }

        private MemoryStream StringToStream(string data)
        {
            var ret = new MemoryStream(Encoding.ASCII.GetBytes(data));

            return ret;
        }
    }
}
