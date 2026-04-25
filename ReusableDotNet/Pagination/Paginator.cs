namespace ReusableDotNet.Pagination;

public sealed class Paginator<T>
{
    private readonly IReadOnlyList<T> _items;
    private int? _defaultPageSize;

    public Paginator(IEnumerable<T> items)
        : this(items, null, PageOutOfRangeBehavior.Empty)
    {
    }

    public Paginator(IEnumerable<T> items, int? defaultPageSize)
        : this(items, defaultPageSize, PageOutOfRangeBehavior.Empty)
    {
    }

    public Paginator(IEnumerable<T> items, int? defaultPageSize, PageOutOfRangeBehavior outOfRangeBehavior)
    {
        ArgumentNullException.ThrowIfNull(items);

        _items = items as IReadOnlyList<T> ?? [.. items];

        if (defaultPageSize.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(defaultPageSize.Value, 0);
        }

        _defaultPageSize = defaultPageSize;
        OutOfRangeBehavior = outOfRangeBehavior;
    }

    public int TotalCount => _items.Count;

    public int? DefaultPageSize => _defaultPageSize;

    public PageOutOfRangeBehavior OutOfRangeBehavior { get; }

    public void SetDefaultPageSize(int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        _defaultPageSize = pageSize;
    }

    public PageResult<T> GetPage(int pageNumber)
    {
        return GetPage(pageNumber, ResolvePageSize());
    }

    public PageResult<T> GetPage(int pageNumber, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageNumber, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        var totalCount = _items.Count;
        var totalPages = CalculateTotalPages(totalCount, pageSize);

        if (totalPages == 0)
        {
            return new PageResult<T>([], pageNumber, pageSize, totalCount, totalPages);
        }

        if (pageNumber > totalPages)
        {
            var resolvedPageNumber = ResolveOutOfRangePageNumber(pageNumber, totalPages);
            if (!resolvedPageNumber.HasValue)
            {
                return new PageResult<T>([], pageNumber, pageSize, totalCount, totalPages);
            }

            pageNumber = resolvedPageNumber.Value;
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

    public PageResult<T> Next(PageResult<T> currentPage)
    {
        ArgumentNullException.ThrowIfNull(currentPage);

        var maxPageNumber = currentPage.TotalPages == 0 ? 1 : currentPage.TotalPages;
        var nextPageNumber = Math.Min(currentPage.PageNumber + 1, maxPageNumber);
        return GetPage(nextPageNumber, currentPage.PageSize);
    }

    public PageResult<T> Previous(PageResult<T> currentPage)
    {
        ArgumentNullException.ThrowIfNull(currentPage);

        var previousPageNumber = Math.Max(1, currentPage.PageNumber - 1);
        return GetPage(previousPageNumber, currentPage.PageSize);
    }

    public PageResult<T> FirstPage()
    {
        return GetPage(1, ResolvePageSize());
    }

    public PageResult<T> FirstPage(int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        return GetPage(1, pageSize);
    }

    public PageResult<T> LastPage()
    {
        return LastPage(ResolvePageSize());
    }

    public PageResult<T> LastPage(int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        var totalPages = CalculateTotalPages(_items.Count, pageSize);
        var lastPageNumber = totalPages == 0 ? 1 : totalPages;
        return GetPage(lastPageNumber, pageSize);
    }

    private int? ResolveOutOfRangePageNumber(int requestedPageNumber, int totalPages)
    {
        return OutOfRangeBehavior switch
        {
            PageOutOfRangeBehavior.Throw => throw new ArgumentOutOfRangeException(nameof(requestedPageNumber), requestedPageNumber, "Page number is greater than total pages."),
            PageOutOfRangeBehavior.ClampToLast => totalPages,
            PageOutOfRangeBehavior.Empty => null,
            _ => throw new ArgumentOutOfRangeException(nameof(OutOfRangeBehavior), OutOfRangeBehavior, "Unsupported out-of-range behavior.")
        };
    }

    private int ResolvePageSize()
    {
        if (_defaultPageSize.HasValue)
        {
            return _defaultPageSize.Value;
        }

        throw new InvalidOperationException("No default page size is configured. Set one at initialization, call SetDefaultPageSize, or use an overload with pageSize.");
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
