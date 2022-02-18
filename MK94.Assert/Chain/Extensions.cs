namespace MK94.Assert.Chain
{
    public static class Extensions
    {
        /// <summary>
        /// Initialises a <see cref="TestChainer"/> based on this <see cref="DiskAsserter"/>
        /// </summary>
        public static TestChainer WithInputs(this DiskAsserter diskAsserter) => new TestChainer(diskAsserter);
    }
}
