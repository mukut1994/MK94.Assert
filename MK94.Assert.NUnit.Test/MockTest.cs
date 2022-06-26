using Microsoft.Extensions.DependencyInjection;
using MK94.Assert.Mocking;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
                .Of<IDatabase>((m) => new Database(), out var database);

            database.Insert(1, "Text 1");
            database.Insert(2, "Text 2");
            database.Insert(3, "Text 3");

            DiskAssert.Matches("Step 1", database.Select(3));
            DiskAssert.Matches("Step 2", database.Select(1));
            DiskAssert.Matches("Step 3", database.Select(2));

            DiskAssert.MatchesSequence();
        }

        [Test]
        public void ServiceProviderTest()
        {
            var services = new ServiceCollection();

            // Add the mocked instance via interface; used by code under test
            // m.CustomContext is set below by "Mock.SetContext(provider)"
            // TODO: This should be an extension method to clean this up a bit
            services.AddSingleton(Mock.Of<IDatabase>(m => (m.CustomContext as IServiceProvider)!.GetRequiredService<Database>()));

            // Add the actual instance via type; used by Mocker in write mode
            // Could come from a different ServiceCollection to keep UT dependendencies cleaner
            services.AddSingleton<Database>();

            var provider = services.BuildServiceProvider();

            // Sets MockContext.CustomContext
            Mock.SetContext(provider);

            // In test mode this just compares to disk and Database is never instantiated
            // In write mode a new Database is instantiated and recorded
            var database = provider.GetRequiredService<IDatabase>();

            database.Insert(10, "Text 10");
            database.Insert(20, "Text 20");
            database.Insert(30, "Text 30");

            DiskAssert.Matches("Service Provider Step 1", database.Select(30));
            DiskAssert.Matches("Service Provider Step 2", database.Select(10));
            DiskAssert.Matches("Service Provider Step 3", database.Select(20));

            DiskAssert.MatchesSequence();
        }
    }
}
