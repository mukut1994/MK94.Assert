using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MK94.Assert
{
    public static class AssertConfigure
    {
        internal static string DefaultGlobalPath => PathRelativeToParentFolder(Assembly.GetExecutingAssembly().GetName().Name, "");
        public static bool WriteMode { get; set; }

        internal static Func<string> PathResolver { get; set; } = () => "";

        internal static Func<string> ChecksumFileResolver { get; set; }

        public static string GlobalPath { get; set; }

        /// <summary>
        /// Safety flag to avoid checking in <see cref="EnableWriteMode"/> by accident <br />
        /// False by default; Set to true on Dev environments (recommended way is via environment variable)
        /// </summary>
        public static bool IsDevEnvironment { get; set; } = false;


        internal static List<Func<string, string>> postProcessors = new List<Func<string, string>>();

        /// <summary>
        /// Changes calls to <see cref="DiskAsserter"/> to write to disk rather than verify <br />
        /// Use this to update breaking tests or add new ones
        /// </summary>
        public static void EnableWriteMode()
        {
            EnsureDevMode();
            WriteMode = true;
        }

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
                .Reverse().Concat(new[] { parentRelative })
                .Aggregate(Path.Combine));
        }

        public static void AddPostProcess(Func<string, string> jsonPostProcessor) => postProcessors.Add(jsonPostProcessor);

        // TODO extended doc link on github
        /// <summary>
        /// Adds a post processor to change Guids to 00000000-0000-0000-0000-000000000000 <br />
        /// This method is primarily meant for legacy projects which aren't easy to change and call <see cref="Guid.NewGuid"/> directly <br />
        /// It is highly recommended to use <see cref="IGuidProvider"/> instead and initialise it to <see cref="PseudoRandomGuidProvider"/> for your UTs and <see cref="GuidProvider"/> for production scenarios
        /// </summary>
        [Obsolete("Use PseudoRandomGuidProvider instead")]
        public static void AddGuidZeroPostProcess()
        {
            AddPostProcess(x => Regex.Replace(x, "[a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12}", Guid.Empty.ToString()));
        }

        public static void AddPathResolver(Func<string> resolver)
        {
            PathResolver = resolver;
        }

        /// <summary>
        /// Defines a file resolver for checksum files. 
        /// The checksum file is used to avoid excessive disk reads.
        /// </summary>
        /// <param name="resolver"></param>
        public static void AddChecksumFileResolver(Func<string> resolver)
        {
            ChecksumFileResolver = resolver;
        }

        internal static void EnsureDevMode()
        {
            if (!IsDevEnvironment)
                throw new InvalidOperationException($"Trying to write during assert but not in a dev environment!!! Make sure EnableWriteMode is not called.");
        }
    }
}
