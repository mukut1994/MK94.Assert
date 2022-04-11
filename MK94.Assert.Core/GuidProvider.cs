using System;

namespace MK94.Assert
{
    /// <summary>
    /// An injectable guid provider
    /// </summary>
    public interface IGuidProvider
    {
        Guid NewGuid();
    }

    /// <summary>
    /// A guid provider running on the <see cref="PseudoRandom"/> settings <br />
    /// Allows repeatable generation of guids <br />
    /// <b>Not for production use.</b> Use <see cref="GuidProvider"/> or implement <see cref="IGuidProvider"/> instead!!!
    /// </summary>
    public class PseudoRandomGuidProvider : IGuidProvider
    {
        private readonly Random random;

        public PseudoRandomGuidProvider() { random = PseudoRandom.GetRandomizer(); }
        public PseudoRandomGuidProvider(Random random) { this.random = random; }

        public Guid NewGuid()
        {
            var buffer = new byte[16];

            random.NextBytes(buffer);

            return new Guid(buffer);
        }
    }

    /// <summary>
    /// A guid provider using the default .net <see cref="Guid.NewGuid"/> <br />
    /// Ideally this class should be implemented at the project to avoid referencing MK94.Assert (<see cref="NewGuid"/> simply returns <see cref="Guid.NewGuid"/>)
    /// </summary>
    public class GuidProvider : IGuidProvider
    {
        public Guid NewGuid()
        {
            return Guid.NewGuid();
        }
    }
}
