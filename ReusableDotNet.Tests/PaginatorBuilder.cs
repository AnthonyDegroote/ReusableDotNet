using ReusableDotNet.Pagination;

namespace ReusableDotNet.Tests;

internal sealed class PaginatorBuilder
{
    private readonly List<int> _items = new();
    private int? _defaultPageSize;
    private PageOutOfRangeBehavior _outOfRangeBehavior = PageOutOfRangeBehavior.Empty;

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

    public PaginatorBuilder WithDefaultPageSize(int pageSize)
    {
        _defaultPageSize = pageSize;
        return this;
    }

    public PaginatorBuilder WithOutOfRangeBehavior(PageOutOfRangeBehavior outOfRangeBehavior)
    {
        _outOfRangeBehavior = outOfRangeBehavior;
        return this;
    }

    public Paginator<int> Build()
    {
        return new Paginator<int>(_items, _defaultPageSize, _outOfRangeBehavior);
    }
}
