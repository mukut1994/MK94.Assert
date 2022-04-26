namespace MK94.Assert.Input
{
    public static class Extensions
    {
        /// <summary>
        /// Initialises a <see cref="TestInput"/> based on this <see cref="DiskAsserter"/>
        /// </summary>
        public static TestInput WithInputs(this DiskAsserter diskAsserter) => new TestInput(diskAsserter);
    }
}
