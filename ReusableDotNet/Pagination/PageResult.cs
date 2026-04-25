namespace ReusableDotNet.Pagination;

public sealed class PageResult<T>
{
    public PageResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount, int totalPages)
    {
        ArgumentNullException.ThrowIfNull(items);

        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = totalPages;
    }

    public IReadOnlyList<T> Items { get; }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages { get; }

    public bool HasPreviousPage => TotalPages > 0 && PageNumber > 1 && PageNumber <= TotalPages;

    public bool HasNextPage => TotalPages > 0 && PageNumber < TotalPages;
}
