using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MK94.Assert
{
    /// <summary>
    /// A source for pseudo random generators
    /// </summary>
    // TODO make it instantiable for better multithread support
    // Similar to the changes to DiskAsserter and Mocker
    public static class PseudoRandom
    {
        /// <summary>
        /// Returns a randomizer derived from the base seed. <br />
        /// The returned randomizer is independent from any subsequent calls to <see cref="GetRandomizer"/> <br />
        /// Useful if the order of some operations are expected to change over the lifetime of the code
        /// </summary>
        public static Random GetRandomizer()
        {
            Contract.Requires(DiskAssert.Default.SeedGenerator != null, $"Set {nameof(DiskAsserter)}.{nameof(DiskAsserter.SeedGenerator)} first");

            var buffer = new byte[4];
            DiskAssert.Default.PseudoRandomizer.NextBytes(buffer);

            return new Random(BitConverter.ToInt32(buffer, 0));
        }

        public static string String(int length = 10, bool noSpecialCharacters = true)
        {
            return DiskAssert.Default.PseudoRandomizer.String(length, noSpecialCharacters);
        }

        public static int Int(int min = int.MinValue, int max = int.MaxValue)
        {
            return DiskAssert.Default.PseudoRandomizer.Int(min, max);
        }

        public static DateTime DateTime(bool includeTime = true, DateTimeKind kind = DateTimeKind.Utc, DateTime? min = null, DateTime? max = null)
        {
            return DiskAssert.Default.PseudoRandomizer.DateTime(includeTime, kind, min, max);
        }
    }

    public static class RandomExtensions
    {
        public static string String(this Random r, int length = 10, bool noSpecialCharacters = true)
        {
            Contract.Requires(length > 0, $"String {nameof(length)} must be at least 1");

            var buffer = new byte[length];

            for (int i = 0; i < length; i++)
            {
                if (noSpecialCharacters)
                    buffer[i] = (byte)r.Next(97, 122);
                else
                    buffer[i] = (byte)r.Next(33, 126);
            }

            return Encoding.ASCII.GetString(buffer);
        }

        public static int Int(this Random r, int min = int.MinValue, int max = int.MaxValue)
        {
            return r.Next(min, max);
        }

        public static DateTime DateTime(this Random r, bool includeTime = true, DateTimeKind kind = DateTimeKind.Utc, DateTime? min = null, DateTime? max = null)
        {
            min = min ?? System.DateTime.MinValue;
            max = max ?? System.DateTime.MaxValue;

            var year = r.Next(min.Value.Year, max.Value.Year);
            var month = r.Next(min.Value.Month, max.Value.Month);
            var day = r.Next(min.Value.Day, Math.Min(System.DateTime.DaysInMonth(year, month), max.Value.Day));
            var hour = 0;
            var minute = 0;
            var second = 0;
            var millisecond = 0;

            if (includeTime)
            {
                hour = r.Next(min.Value.Hour, max.Value.Hour);
                minute = r.Next(min.Value.Minute, max.Value.Minute);
                second = r.Next(min.Value.Second, max.Value.Second);
                millisecond = r.Next(min.Value.Millisecond, max.Value.Millisecond);
            }

            return new DateTime(year, month, day, hour, minute, second, millisecond, kind);
        }
    }

    /// <summary>
    /// A source for pseudo random generators
    /// </summary>
    // TODO make it instantiable for better multithread support
    // Similar to the changes to DiskAsserter and Mocker
    public class PseudoRandomizer
    {
        private Random randomizer;
        private Random stringRandomizer;
        private Random numberRandomizer;
        private Random dateRandomizer;
        private ConcurrentDictionary<string, Random> namedRandomizers = new ConcurrentDictionary<string, Random>();

        public static AsyncLocal<PseudoRandomizer> Default = new AsyncLocal<PseudoRandomizer>();

        public PseudoRandomizer(string seed)
        {
            seed = seed.Replace(System.IO.Path.DirectorySeparatorChar, ' ').Replace(System.IO.Path.DirectorySeparatorChar, ' ');

            Contract.Requires(!string.IsNullOrEmpty(seed), $"{nameof(seed)} cannot be null or empty");

            var intSeed = BitConverter.ToInt32(SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(seed)).Take(4).ToArray(), 0);

            randomizer = new Random(intSeed);
            stringRandomizer = new Random(randomizer.Next());
            numberRandomizer = new Random(randomizer.Next());
            dateRandomizer = new Random(randomizer.Next());
        }

        /// <summary>
        /// Returns a randomizer derived from the base seed. <br />
        /// It's recommended to call GetRandomizer with a name every time a new value is needed 
        /// for good multi-threaded and <see cref="DiskAssertSetupExtensions.WithSetup(DiskAsserter, Action)"/> support.
        /// <paramref name="name">A unique name for this randomizer so subsequent calls to GetRandomizer can access the same instance</paramref>
        /// </summary>
        public Random GetRandomizer(string? name = null)
        {
            if (name != null)
                return namedRandomizers.GetOrAdd(name, (x) => new Random(randomizer.Int()));
            
            var buffer = new byte[4];
            randomizer.NextBytes(buffer);

            return new Random(BitConverter.ToInt32(buffer, 0));
        }

        public void NextBytes(byte[] buffer)
        {
            randomizer.NextBytes(buffer);
        }

        public string String(int length = 10, bool noSpecialCharacters = true)
        {
            return stringRandomizer.String(length, noSpecialCharacters);
        }

        public int Int(int min = int.MinValue, int max = int.MaxValue)
        {
            return numberRandomizer.Next(min, max);
        }

        public DateTime DateTime(bool includeTime = true, DateTimeKind kind = DateTimeKind.Utc, DateTime? min = null, DateTime? max = null)
        {
            return dateRandomizer.DateTime(includeTime, kind, min, max);
        }
    }
}
