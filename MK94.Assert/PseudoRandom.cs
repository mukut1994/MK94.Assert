using System;
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
    public static class PseudoRandom
    {
        private class Instance
        {
            public string oldSeed;
            public Random randomizer;
            public Random stringRandomizer;
            public Random numberRandomizer;
            public Random dateRandomizer;
        }

        private static Func<string> seedGenerator;

        private static AsyncLocal<Instance> instance = new AsyncLocal<Instance>();

        public static void WithBaseSeed(Func<string> seedGenerator)
        {
            Contract.Requires(seedGenerator != null, $"{nameof(seedGenerator)} cannot be null or empty");

            PseudoRandom.seedGenerator = seedGenerator;
        }

        private static void CheckDynamicSeedChanged()
        {
            if (instance.Value == null)
                instance.Value = new Instance();

            if (seedGenerator == null)
                throw new InvalidProgramException("Setup WithPseudoRandom");

            var newSeed = seedGenerator();
            if (newSeed == instance.Value.oldSeed)
                return;

            instance.Value.oldSeed = newSeed;
            WithBaseSeed(newSeed);
        }

        /// <summary>
        /// Sets the seed for randomizer. Has to be called before anything else and ideally at test initialze
        /// </summary>
        /// <param name="seed">The seed value. Currently executing test name is recommended</param>
        public static void WithBaseSeed(string seed)
        {
            seed = seed.Replace(System.IO.Path.DirectorySeparatorChar, ' ').Replace(System.IO.Path.DirectorySeparatorChar, ' ');
            
            Contract.Requires(!string.IsNullOrEmpty(seed), $"{nameof(seed)} cannot be null or empty");

            var intSeed = BitConverter.ToInt32(SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(seed)).Take(4).ToArray(), 0);

            instance.Value.randomizer = new Random(intSeed);

            instance.Value.stringRandomizer = GetRandomizer();
            instance.Value.numberRandomizer = GetRandomizer();
            instance.Value.dateRandomizer = GetRandomizer();
        }

        /// <summary>
        /// Returns a randomizer derived from the base seed. <br />
        /// The returned randomizer is independent from any subsequent calls to <see cref="GetRandomizer"/> <br />
        /// Useful if the order of some operations are expected to change over the lifetime of the code
        /// </summary>
        public static Random GetRandomizer()
        {
            CheckDynamicSeedChanged();

            Contract.Requires(instance.Value.randomizer != null, $"Call {nameof(PseudoRandom)}.{nameof(WithBaseSeed)} first");

            var buffer = new byte[4];
            instance.Value.randomizer.NextBytes(buffer);

            return new Random(BitConverter.ToInt32(buffer, 0));
        }

        public static string String(int length = 10, bool noSpecialCharacters = true)
        {
            CheckDynamicSeedChanged();
            return instance.Value.stringRandomizer.String(length, noSpecialCharacters);
        }

        public static int Int(int min = int.MinValue, int max = int.MaxValue)
        {
            CheckDynamicSeedChanged();
            return instance.Value.numberRandomizer.Int(min, max);
        }

        public static DateTime DateTime(bool includeTime = true, DateTimeKind kind = DateTimeKind.Utc, DateTime? min = null, DateTime? max = null)
        {
            CheckDynamicSeedChanged();
            return instance.Value.dateRandomizer.DateTime(includeTime, kind, min, max);
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
}
