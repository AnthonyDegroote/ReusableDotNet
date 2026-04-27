using ReusableDotNet.Tests.Builders;

namespace ReusableDotNet.Tests;

public class CacheExecutorTests
{
    [Fact]
    public void Execute_ShouldCacheValue_ForSameKey()
    {
        var cache = new CacheExecutorBuilder().Build();
        var callCount = 0;
        const string key = "abc";
        const int expected = 3;

        int ValueFactory(string cacheKey)
        {
            callCount++;
            return cacheKey.Length;
        }

        var first = cache.Execute(key, ValueFactory);
        var second = cache.Execute(key, ValueFactory);

        Assert.Equal(expected, first);
        Assert.Equal(expected, second);
        Assert.Equal(1, callCount);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void TryGetValue_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        var cache = new CacheExecutorBuilder().Build();

        var found = cache.TryGetValue("missing", out var value);

        Assert.False(found);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryGetValue_ShouldReturnTrue_AfterExecute()
    {
        var cache = new CacheExecutorBuilder().Build();
        const string key = "hello";
        const int expected = 5;

        int ValueFactory(string cacheKey)
        {
            return cacheKey.Length;
        }

        cache.Execute(key, ValueFactory);
        var found = cache.TryGetValue(key, out var value);

        Assert.True(found);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void Invalidate_ShouldRemoveKey_AndRecomputeOnNextExecute()
    {
        var cache = new CacheExecutorBuilder().Build();
        var callCount = 0;
        const string key = "x";
        const int firstExpected = 2;
        const int secondExpected = 3;

        int ValueFactory(string cacheKey)
        {
            callCount++;
            return cacheKey.Length + callCount;
        }

        var first = cache.Execute(key, ValueFactory);
        cache.Invalidate(key);
        var second = cache.Execute(key, ValueFactory);

        Assert.Equal(firstExpected, first);
        Assert.Equal(secondExpected, second);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Execute_ShouldNotCacheFaultedValue_AndAllowRetry()
    {
        var cache = new CacheExecutorBuilder().Build();
        var callCount = 0;
        const string key = "retry";
        const int expected = 5;

        int ValueFactory(string cacheKey)
        {
            callCount++;
            if (callCount == 1)
            {
                throw new InvalidOperationException("first call fails");
            }

            return cacheKey.Length;
        }

        void Act()
        {
            cache.Execute(key, ValueFactory);
        }

        Assert.Throws<InvalidOperationException>(Act);

        var value = cache.Execute(key, ValueFactory);

        Assert.Equal(expected, value);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Clear_ShouldRemoveAllEntries()
    {
        var cache = new CacheExecutorBuilder()
            .WithValue("a", 1)
            .WithValue("bb", 2)
            .Build();

        Assert.Equal(2, cache.Count);

        cache.Clear();

        Assert.Equal(0, cache.Count);
        Assert.Empty(cache.Keys);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenInitialValuesFactoryIsNull()
    {
        Func<IEnumerable<KeyValuePair<string, int>>>? factory = null;

        void Act()
        {
            _ = new CacheExecutor<string, int>(factory!);
        }

        Assert.Throws<ArgumentNullException>(Act);
    }

    [Fact]
    public void Constructor_ShouldInitializeCache_FromInitialValuesFactory()
    {
        var cache = new CacheExecutor<string, int>(CreateInitialValues);

        Assert.Equal(2, cache.Count);
        Assert.Contains("a", cache.Keys);
        Assert.Contains("bb", cache.Keys);

        var first = cache.Execute("a", FallbackFactory);
        var second = cache.Execute("bb", FallbackFactory);

        Assert.Equal(1, first);
        Assert.Equal(2, second);

        static IEnumerable<KeyValuePair<string, int>> CreateInitialValues()
        {
            yield return new KeyValuePair<string, int>("a", 1);
            yield return new KeyValuePair<string, int>("bb", 2);
        }

        static int FallbackFactory(string key)
        {
            return key.Length + 100;
        }
    }
}
