
using System.Collections.Concurrent;

namespace MK94.Assert.Example.PizzaApi;

/// <summary>
/// An object to hold our application state. <br />
/// Can be anything in production e.g. MySql, mssql, oracle sql, files on disk etc <br />
/// As long as we can mock it. <br />
/// If the driver does not support mocking then wrapper classes like this one are always an option.
/// </summary>
public interface IDocumentResource<TId, TType>
    where TId : notnull
{
    Task Upsert(TId id, TType value);

    Task<TType> Get(TId id);

    Task<List<TType>> List();

    Task Delete(TId id);
}

/// <summary>
/// An in memory implementation to hold state
/// </summary>
public class InMemoryDocumentResource<TId, TType> : IDocumentResource<TId, TType>
    where TId : notnull
{
    private readonly ConcurrentDictionary<TId, TType> db = new();

    public Task<TType> Get(TId id)
    {
        return Task.FromResult(db[id]);
    }

    public Task<TType?> GetOrDefault(TId id)
    {
        var ret = db.GetValueOrDefault(id);

        return Task.FromResult(ret);
    }

    public Task<List<TType>> List()
    {
        var ret = db.Values
            .ToList();

        return Task.FromResult(ret);
    }

    public Task<Dictionary<TId, TType>> Dictionary()
    {
        var ret = db
            .ToDictionary(x => x.Key, x => x.Value);

        return Task.FromResult(ret);
    }


    public Task Upsert(TId id, TType value)
    {
        db[id] = value;

        return Task.CompletedTask;
    }

    public Task Delete(TId id)
    {
        db.TryRemove(id, out _);

        return Task.CompletedTask;
    }
}
    