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

            var previousSetupMode = diskAsserter.InSetup;
            var previousWriteMode = diskAsserter.WriteMode;

            diskAsserter.WriteMode = false;
            diskAsserter.InSetup = true;

            await task();

            diskAsserter.WriteMode = previousWriteMode;
            diskAsserter.InSetup = previousSetupMode;

            return diskAsserter;
        }

        /// <inheritdoc cref="WithSetup(DiskAsserter, Func{Task})"/>
        public static DiskAsserter WithSetup(this DiskAsserter diskAsserter, Action task)
        {
            if (!diskAsserter.WriteMode && !diskAsserter.InSetup)
                return diskAsserter;

            var previousSetupMode = diskAsserter.InSetup;
            var previousWriteMode = diskAsserter.WriteMode;
            var previousSeedGenerator = PseudoRandom.SeedGenerator;

            diskAsserter.WriteMode = false;
            diskAsserter.InSetup = true;
            PseudoRandom.SeedGenerator = () => "SETUP" + previousSeedGenerator;

            task();

            diskAsserter.WriteMode = previousWriteMode;
            diskAsserter.InSetup = previousSetupMode;
            PseudoRandom.SeedGenerator = previousSeedGenerator;

            return diskAsserter;
        }
    }
}
