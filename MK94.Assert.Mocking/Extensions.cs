namespace MK94.Assert.Mocking
{
    public static class Extensions
    {
        /// <summary>
        /// Creates an instance of a <see cref="Mocker"/>
        /// </summary>
        /// <param name="diskAsserter">The <see cref="DiskAsserter"/> to use for sequence mapping</param>
        /// <returns>A new instance of a <see cref="Mocker"/></returns>
        public static Mocker WithMocks(this DiskAsserter diskAsserter) => new Mocker(diskAsserter);
    }
}
