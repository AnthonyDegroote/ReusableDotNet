using ReusableDotNet.Pagination;

namespace ReusableDotNet.Tests;

internal sealed class PaginatorBuilder
{
    private readonly List<int> _items = new();

    public PaginatorBuilder WithItems(params int[] items)
    {
        _items.AddRange(items);
        return this;
    }

    public PaginatorBuilder WithRange(int start, int count)
    {
        _items.AddRange(Enumerable.Range(start, count));
        return this;
    }

    public Paginator<int> Build()
    {
        return new Paginator<int>(_items);
    }
}
