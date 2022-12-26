using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK94.Assert.NUnit.Test
{
    public interface IDatabase
    {
        Task Insert(int id, string text);

        Task<string> Select(int id);
    }

    public class Database : IDatabase
    {
        private Dictionary<int, string> fakeDb = new();

        public Task Insert(int id, string text)
        {
            fakeDb[id] = text;

            return Task.CompletedTask;
        }

        public Task<string> Select(int id)
        {
            return Task.FromResult(fakeDb[id]);
        }
    }
}
