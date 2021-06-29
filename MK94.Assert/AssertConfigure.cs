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
        internal static string DefaultGlobalPath => PathRelativeToParentFolder(Assembly.GetExecutingAssembly().GetName().Name, "Testdata");
        internal static bool WriteMode { get; set; }

        public static Func<AssertContext, string> PathResolver { get; set; }
        public static string GlobalPath { get; set; }

        /// <summary>
        /// Safety flag to avoid checking in <see cref="EnableWriteMode"/> by accident <br />
        /// False by default; Set to true on Dev environments (recommended way is via environment variable)
        /// </summary>
        public static bool IsDevEnvironment { get; set; } = false;


        private static List<Func<string, string>> postProcessors = new List<Func<string, string>>();

        /// <summary>
        /// Changes calls to <see cref="DiskAssert"/> to write to disk rather than verify <br />
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
        /// <param name="parentRelative">The folder </param>
        public static string PathRelativeToParentFolder(string parentFolder, string parentRelative)
        {
            var dirs = Directory.GetCurrentDirectory().Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            for (int i = dirs.Length - 1; i > 0; i--)
            {
                if (dirs[i] == parentFolder)
                    return Path.GetFullPath(Path.Combine("/", dirs
                        .Take(dirs.Length - i + 1)
                        .Concat(new[] { parentRelative })
                        .Aggregate(Path.Combine)));
            }

            throw new InvalidProgramException($"Parent directory '{parentFolder}' does not exist under {Directory.GetCurrentDirectory()}");
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

        internal static void EnsureDevMode()
        {
            if (!IsDevEnvironment)
                throw new InvalidOperationException($"Trying to write during assert but not in a dev environment!!! Make sure EnableWriteMode is not called.");
        }

    }

    /// <summary>
    /// Helper class to change <see cref="AssertConfigure"/> into a builder pattern
    /// </summary>
    public class Configuration
    {
        internal Configuration() { }
    }

    public struct AssertContext
    {
        public string StepName { get; }

        public AssertContext(string stepName)
        {
            StepName = stepName;
        }
    }
}
