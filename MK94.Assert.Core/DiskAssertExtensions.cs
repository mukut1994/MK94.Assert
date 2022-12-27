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

            PreRunConfigureContext(diskAsserter, out var previousSetupMode, out var previousWriteMode, out var previousSeedGenerator);

            await task();

            PostRunRestoreContext(diskAsserter, previousSetupMode, previousWriteMode, previousSeedGenerator);

            return diskAsserter;
        }

        /// <inheritdoc cref="WithSetup(DiskAsserter, Func{Task})"/>
        public static DiskAsserter WithSetup(this DiskAsserter diskAsserter, Action task)
        {
            if (!ShouldRunSetup(diskAsserter))
                return diskAsserter;

            PreRunConfigureContext(diskAsserter, out var previousSetupMode, out var previousWriteMode, out var previousSeedGenerator);

            task();

            diskAsserter.WriteMode = previousWriteMode;
            diskAsserter.InSetup = previousSetupMode;
            PseudoRandom.SeedGenerator = previousSeedGenerator;

            return diskAsserter;
        }

        private static void PreRunConfigureContext(DiskAsserter diskAsserter, out bool previousSetupMode, out bool previousWriteMode, out Func<string> previousSeedGenerator)
        {
            previousSetupMode = diskAsserter.InSetup;
            previousWriteMode = diskAsserter.WriteMode;
            previousSeedGenerator = PseudoRandom.SeedGenerator;

            var seedAppend = PseudoRandom.SeedGenerator;

            diskAsserter.WriteMode = false;
            diskAsserter.InSetup = true;
            PseudoRandom.SeedGenerator = () => "SETUP" + seedAppend;
        }

        private static void PostRunRestoreContext(DiskAsserter diskAsserter, bool previousSetupMode, bool previousWriteMode, Func<string> previousSeedGenerator)
        {
            diskAsserter.WriteMode = previousWriteMode;
            diskAsserter.InSetup = previousSetupMode;
            PseudoRandom.SeedGenerator = previousSeedGenerator;
        }

        private static bool ShouldRunSetup(DiskAsserter diskAsserter)
        {
            return diskAsserter.WriteMode || diskAsserter.InSetup;
        }
    }
}
