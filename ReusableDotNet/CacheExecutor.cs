using System.Collections.Concurrent;

namespace ReusableDotNet;

public class CacheExecutor<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _cache = new();

    public ICollection<TKey> Keys => _cache.Keys;

    public int Count => _cache.Count;

    public CacheExecutor()
    {
        // No initialization needed for the default constructor.
    }

    public CacheExecutor(Func<IEnumerable<KeyValuePair<TKey, TValue>>> initialValuesFactory)
    {
        ArgumentNullException.ThrowIfNull(initialValuesFactory);

        foreach (var (key, value) in initialValuesFactory())
        {
            var lazy = new Lazy<TValue>(() => value, LazyThreadSafetyMode.ExecutionAndPublication);
            _cache[key] = lazy;
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_cache.TryGetValue(key, out var lazyValue) && lazyValue.IsValueCreated)
        {
            value = lazyValue.Value;
            return true;
        }

        value = default!;
        return false;
    }

    public TValue Execute(TKey key, Func<TKey, TValue> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(valueFactory);

        var lazyValue = _cache.GetOrAdd(key, CreateLazy, valueFactory);

        try
        {
            return lazyValue.Value;
        }
        catch
        {
            _cache.TryRemove(new KeyValuePair<TKey, Lazy<TValue>>(key, lazyValue));
            throw;
        }

        static Lazy<TValue> CreateLazy(TKey cacheKey, Func<TKey, TValue> factory)
        {
            return new Lazy<TValue>(CreateValue, LazyThreadSafetyMode.ExecutionAndPublication);

            TValue CreateValue() => factory(cacheKey);
        }
    }

    public void Invalidate(TKey key)
    {
        _cache.TryRemove(key, out _);
    }

    public void Clear()
    {
        _cache.Clear();
    }
}
