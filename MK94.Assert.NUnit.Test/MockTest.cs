using MK94.Assert.Mocking;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK94.Assert.NUnit.Test
{
    public class MockTest
    {
        public interface IDatabase
        {
            void Insert(int id, string text);

            string Select(int id);
        }

        private class Database : IDatabase
        {
            private Dictionary<int, string> fakeDb = new();

            public void Insert(int id, string text)
            {
                fakeDb[id] = text;
            }

            public string Select(int id)
            {
                return fakeDb[id];
            }
        }

        [Test]
        public void BasicTest()
        { 
            DiskAssert.Default
                .WithMocks()
                .Of<IDatabase>(() => new Database(), out var database);

            database.Insert(1, "Text 1");
            database.Insert(2, "Text 2");
            database.Insert(3, "Text 3");

            DiskAssert.Matches("Step 1", database.Select(3));
            DiskAssert.Matches("Step 2", database.Select(1));
            DiskAssert.Matches("Step 3", database.Select(2));

            DiskAssert.MatchesSequence();
        }
    }
}
