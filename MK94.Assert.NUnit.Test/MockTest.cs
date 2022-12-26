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
        [Test]
        public async Task BasicTest()
        {
            DiskAssert.Default
                .WithMocks()
                .Of<IDatabase>((m) => new Database(), out var database);

            await database.Insert(1, "Text 1");
            await database.Insert(2, "Text 2");
            await database.Insert(3, "Text 3");

            DiskAssert.Matches("Step 1", await database.Select(3));
            DiskAssert.Matches("Step 2", await database.Select(1));
            DiskAssert.Matches("Step 3", await database.Select(2));

            DiskAssert.MatchesSequence();
        }

        [Test]
        public async Task ServiceProviderTest()
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

            await database.Insert(10, "Text 10");
            await database.Insert(20, "Text 20");
            await database.Insert(30, "Text 30");

            DiskAssert.Matches("Service Provider Step 1", await database.Select(30));
            DiskAssert.Matches("Service Provider Step 2", await database.Select(10));
            DiskAssert.Matches("Service Provider Step 3", await database.Select(20));

            DiskAssert.MatchesSequence();
        }

        [Test]
        public async Task GenericTest()
        {
            DiskAssert.Default
                .WithMocks()
                .Of<IGenericDatabase<int>>((m) => new Database<int>(), out var database);

            await database.Insert(1, 11);

            DiskAssert.Matches("Step 1", await database.Select(1));

            DiskAssert.MatchesSequence();
        }
    }
}
