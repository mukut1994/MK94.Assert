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
        /// <param name="diskAsserter">The asserter to run setup against</param>
        /// <param name="task">The code that runs any setup code for the main test</param>
        /// <param name="forceExecuteSetup">
        /// By default, the setup is only executed when we're in write mode.
        /// We can avoid re-running setup on non-write tests because we have the output from previous runs.
        /// If for some reason we cannot save the output from the previous run e.g. a class cannot be mocked
        /// this method allows the setup to run, but without writing the output to disk.
        /// </param>
        public static async Task<DiskAsserter> WithSetup(
            this DiskAsserter diskAsserter,
            Func<Task> task,
            bool forceExecuteSetup = false)
        {
            if (!forceExecuteSetup && !ShouldRunSetup(diskAsserter))
                return diskAsserter;

            PreRunConfigureContext(diskAsserter, out var previousSetupMode, out var previousWriteMode, out var previousSeedGenerator, out var previousPseudoRandomizer);

            await task();

            PostRunRestoreContext(diskAsserter, previousSetupMode, previousWriteMode, previousSeedGenerator, previousPseudoRandomizer);

            return diskAsserter;
        }

        /// <inheritdoc cref="WithSetup(DiskAsserter, Func{Task})"/>
        public static DiskAsserter WithSetup(
            this DiskAsserter diskAsserter,
            Action task,
            bool forceExecuteSetup = false)
        {
            if (!forceExecuteSetup && !ShouldRunSetup(diskAsserter))
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
            diskAsserter.SeedGenerator = () => "SETUP" + seedAppend();
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
