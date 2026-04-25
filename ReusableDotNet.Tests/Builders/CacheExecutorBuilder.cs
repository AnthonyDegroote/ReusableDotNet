namespace ReusableDotNet.Tests.Builders;

internal sealed class CacheExecutorBuilder
{
    private readonly CacheExecutor<string, int> _cache = new();

    public CacheExecutorBuilder WithValue(string key, int value)
    {
        _cache.Execute(key, CreateValueFactory(value));
        return this;
    }

    public CacheExecutor<string, int> Build()
    {
        return _cache;
    }

    private static Func<string, int> CreateValueFactory(int value)
    {
        return ValueFactory;

        int ValueFactory(string _)
        {
            return value;
        }
    }
}
