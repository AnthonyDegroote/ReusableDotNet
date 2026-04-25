using ReusableDotNet.Pagination;

namespace ReusableDotNet.Tests;

public class PaginatorTests
{
    [Fact]
    public void GetPage_ShouldReturnItems_ForFirstPage()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .Build();

        var page = paginator.GetPage(1, 3);

        Assert.Equal([1, 2, 3], page.Items);
        Assert.Equal(1, page.PageNumber);
        Assert.Equal(3, page.PageSize);
        Assert.Equal(10, page.TotalCount);
        Assert.Equal(4, page.TotalPages);
        Assert.False(page.HasPreviousPage);
        Assert.True(page.HasNextPage);
    }

    [Fact]
    public void GetPage_ShouldReturnRemainingItems_ForLastPage()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .Build();

        var page = paginator.GetPage(4, 3);

        Assert.Equal([10], page.Items);
        Assert.False(page.HasNextPage);
        Assert.True(page.HasPreviousPage);
    }

    [Fact]
    public void GetPage_ShouldReturnEmpty_WhenPageNumberIsGreaterThanTotalPages()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 5)
            .Build();

        var page = paginator.GetPage(3, 3);

        Assert.Empty(page.Items);
        Assert.Equal(3, page.PageNumber);
        Assert.Equal(3, page.PageSize);
        Assert.Equal(5, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
        Assert.False(page.HasPreviousPage);
        Assert.False(page.HasNextPage);
    }

    [Fact]
    public void GetPage_ShouldReturnEmpty_WhenSourceIsEmpty()
    {
        var paginator = new PaginatorBuilder().Build();

        var page = paginator.GetPage(1, 10);

        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalCount);
        Assert.Equal(0, page.TotalPages);
        Assert.False(page.HasPreviousPage);
        Assert.False(page.HasNextPage);
    }

    [Fact]
    public void GetPage_ShouldThrow_WhenPageNumberIsInvalid()
    {
        var paginator = new PaginatorBuilder()
            .WithItems(1, 2, 3)
            .Build();

        void Act()
        {
            paginator.GetPage(0, 2);
        }

        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }

    [Fact]
    public void GetPage_ShouldThrow_WhenPageSizeIsInvalid()
    {
        var paginator = new PaginatorBuilder()
            .WithItems(1, 2, 3)
            .Build();

        void Act()
        {
            paginator.GetPage(1, 0);
        }

        Assert.Throws<ArgumentOutOfRangeException>(Act);
    }
}
