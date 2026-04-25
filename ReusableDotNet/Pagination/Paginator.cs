namespace ReusableDotNet.Pagination;

public sealed class Paginator<T>
{
    private readonly IReadOnlyList<T> _items;

    public Paginator(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _items = items as IReadOnlyList<T> ?? items.ToList();
    }

    public int TotalCount => _items.Count;

    public PageResult<T> GetPage(int pageNumber, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageNumber, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        var totalCount = _items.Count;
        var totalPages = CalculateTotalPages(totalCount, pageSize);

        if (totalPages == 0 || pageNumber > totalPages)
        {
            return new PageResult<T>(Array.Empty<T>(), pageNumber, pageSize, totalCount, totalPages);
        }

        var startIndex = (pageNumber - 1) * pageSize;
        var count = Math.Min(pageSize, totalCount - startIndex);
        var pageItems = CopyItems(startIndex, count);

        return new PageResult<T>(pageItems, pageNumber, pageSize, totalCount, totalPages);

        IReadOnlyList<T> CopyItems(int index, int length)
        {
            if (_items is List<T> list)
            {
                return list.GetRange(index, length);
            }

            var result = new List<T>(length);
            for (var i = 0; i < length; i++)
            {
                result.Add(_items[index + i]);
            }

            return result;
        }
    }

    private static int CalculateTotalPages(int totalCount, int pageSize)
    {
        if (totalCount == 0)
        {
            return 0;
        }

        return (totalCount + pageSize - 1) / pageSize;
    }
}
