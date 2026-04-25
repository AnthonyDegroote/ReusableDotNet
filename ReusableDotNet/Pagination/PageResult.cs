namespace ReusableDotNet.Pagination;

public sealed record class PageResult<T>
{
    internal PageResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount, int totalPages, object? cursor)
    {
        ArgumentNullException.ThrowIfNull(items);

        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalPages;
        Cursor = cursor;
    }

    public PageResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount, int totalPages)
        : this(items, pageNumber, pageSize, totalCount, totalPages, null)
    {
    }

    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }

    internal object? Cursor { get; }

    public bool IsEmpty => Items.Count == 0;

    public int FirstItemIndex => IsEmpty ? 0 : ((PageNumber - 1) * PageSize) + 1;

    public int LastItemIndex => IsEmpty ? 0 : FirstItemIndex + Items.Count - 1;

    public int RemainingItemCount => IsEmpty ? TotalCount : Math.Max(0, TotalCount - LastItemIndex);

    public bool HasPreviousPage => TotalPages > 0 && PageNumber > 1 && PageNumber <= TotalPages;

    public bool HasNextPage => TotalPages > 0 && PageNumber < TotalPages;
}
