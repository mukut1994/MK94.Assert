using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK94.Assert
{
    public static class DiskAssertSetupExtensions
    {
        /// <summary>
        /// Runs code for setting up the current test. <br />
        /// This sets the <see cref="DiskAsserter.InSetup"/> flag and disables any calls to <see cref="DiskAsserter.Matches{T}(string, T)"/> or related calls. <br />
        /// Useful for chaining many tests together where previous tests create objects the current test wants to make use of.
        /// </summary>
        public static async Task<DiskAsserter> WithSetup(this DiskAsserter diskAsserter, Func<Task> task)
        {
            if (!diskAsserter.WriteMode && !diskAsserter.InSetup)
                return diskAsserter;

            PreRunConfigureContext(diskAsserter, out var previousSetupMode, out var previousWriteMode, out var previousSeedGenerator, out var previousPseudoRandomizer);

            await task();

            PostRunRestoreContext(diskAsserter, previousSetupMode, previousWriteMode, previousSeedGenerator, previousPseudoRandomizer);

            return diskAsserter;
        }

        /// <inheritdoc cref="WithSetup(DiskAsserter, Func{Task})"/>
        public static DiskAsserter WithSetup(this DiskAsserter diskAsserter, Action task)
        {
            if (!ShouldRunSetup(diskAsserter))
                return diskAsserter;

            PreRunConfigureContext(diskAsserter, out var previousSetupMode, out var previousWriteMode, out var previousSeedGenerator, out var previousPseudoRandomizer);

            task();

            PostRunRestoreContext(diskAsserter, previousSetupMode, previousWriteMode, previousSeedGenerator, previousPseudoRandomizer);

            return diskAsserter;
        }

        private static void PreRunConfigureContext(DiskAsserter diskAsserter, out bool previousSetupMode, 
            out bool previousWriteMode, out Func<string> previousSeedGenerator,
            out PseudoRandomizer previousRandomizer)
        {
            previousSetupMode = diskAsserter.InSetup;
            previousWriteMode = diskAsserter.WriteMode;
            previousSeedGenerator = diskAsserter.SeedGenerator;
            previousRandomizer = diskAsserter.PseudoRandomizer;

            var seedAppend = diskAsserter.SeedGenerator;

            diskAsserter.WriteMode = false;
            diskAsserter.InSetup = true;
            diskAsserter.SeedGenerator = () => "SETUP" + seedAppend;
            diskAsserter.PseudoRandomizer = new PseudoRandomizer(diskAsserter.SeedGenerator());
        }

        private static void PostRunRestoreContext(DiskAsserter diskAsserter, bool previousSetupMode,
            bool previousWriteMode, Func<string> previousSeedGenerator,
            PseudoRandomizer previousRandomizer)
        {
            diskAsserter.WriteMode = previousWriteMode;
            diskAsserter.InSetup = previousSetupMode;
            diskAsserter.SeedGenerator = previousSeedGenerator;
            diskAsserter.PseudoRandomizer = previousRandomizer;
        }

        private static bool ShouldRunSetup(DiskAsserter diskAsserter)
        {
            return diskAsserter.WriteMode || diskAsserter.InSetup;
        }
    }
}
