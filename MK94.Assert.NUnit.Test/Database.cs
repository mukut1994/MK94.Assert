using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MK94.Assert.NUnit.Test
{
    public interface IDatabase
    {
        Task Insert(int id, string text);

        Task<string> Select(int id);

        Task<IReadOnlyDictionary<int, string>> Everything();
    }

    public class Database : IDatabase
    {
        private Dictionary<int, string> fakeDb = new();

        public Task<IReadOnlyDictionary<int, string>> Everything()
        {
            return Task.FromResult((IReadOnlyDictionary<int, string>) fakeDb);
        }

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


    public interface IGenericDatabase<TType>
    {
        Task Insert(int id, TType text);

        Task<TType> Select(int id);
    }

    public class Database<TType> : IGenericDatabase<TType>
    {
        private Dictionary<int, TType> fakeDb = new();

        public Task Insert(int id, TType value)
        {
            fakeDb[id] = value;

            return Task.CompletedTask;
        }

        public Task<TType> Select(int id)
        {
            return Task.FromResult(fakeDb[id]);
        }
    }
}
