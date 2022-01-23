using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MK94.Assert
{
    public static class PathHelper
    {
        /// <summary>
        /// Helper method to set the global path relative to dll/exe path. <br />
        /// Goes relative up until it finds parent folder then appends the relative path <br />
        /// E.g. if the dll is in "/user/code/projectName/src/bin/netcorapp/", parentFolder = "code", parentRelative = "testData" <br />
        /// The global path is set to "/user/code/testData"
        /// </summary>
        /// <param name="parentFolder">The parent folder relative to the dll/exe</param>
        /// <param name="parentRelative">The folder</param>
        public static string PathRelativeToParentFolder(string parentFolder, string parentRelative)
        {
            var dirs = Directory.GetCurrentDirectory().Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (dirs.All(d => d != parentFolder))
                throw new InvalidProgramException($"Parent directory '{parentFolder}' does not exist under {Directory.GetCurrentDirectory()}");

            return Path.Combine("/", dirs
                .Reverse()
                .SkipWhile(x => x != parentFolder)
                .Reverse()
                .Concat(new[] { parentRelative })
                .Aggregate(Path.Combine));
        }
    }
}
