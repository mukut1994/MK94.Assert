using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MK94.Assert.NUnit.MatrixTest
{
    public class InMemoryDb
    {
        private Dictionary<string, List<object>> dbs = new Dictionary<string, List<object>>();

        private int CallCount = 0;

        public void Load<T>(Action<T> a)
        {
            dbs = ScenarioTest.LoadFromStage(a, "db").As<Dictionary<string, List<object>>>();
        }

        public void Store()
        {
            ScenarioTest.Assert(dbs, "db");
        }

        public void Insert<T>(T obj)
        {
            ScenarioTest.Assert(obj, $"{CallCount++}_DbInsert{typeof(T).Name}");

            var db = GetDb<T>();

            db.Add(obj);
        }

        public void Delete<T>(Predicate<T> where)
        {
            var db = GetDb<T>();

            var deleted = db.RemoveAll(x => where((T)x));

            ScenarioTest.Assert(deleted, $"{CallCount++}_DbDelete{typeof(T).Name}");
        }

        public List<T> Select<T>(Predicate<T> where)
        {
            var db = GetDb<T>();

            return db
                .Select(x => x is JsonElement j ? JsonSerializer.Deserialize<T>(j.GetRawText()) : x)
                .Where(x => where((T)x))
                .Cast<T>().ToList();
        }

        private List<object> GetDb<T>()
        {
            if (dbs.TryGetValue(typeof(T).FullName, out var e))
                return e;

            var ret = new List<object>();

            dbs[typeof(T).FullName] = ret;
            return ret;
        }
    }
}
