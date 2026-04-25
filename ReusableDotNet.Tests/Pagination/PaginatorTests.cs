using ReusableDotNet.Pagination;
using ReusableDotNet.Tests.Builders;

namespace ReusableDotNet.Tests.Pagination;

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
    public void GetPage_ShouldReturnEmpty_WhenPageNumberIsGreaterThanTotalPages_WithDefaultBehavior()
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
    public void GetPage_ShouldClampToLast_WhenPageNumberIsGreaterThanTotalPages_AndBehaviorIsClampToLast()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 5)
            .WithOutOfRangeBehavior(PageOutOfRangeBehavior.ClampToLast)
            .Build();

        var page = paginator.GetPage(3, 3);

        Assert.Equal([4, 5], page.Items);
        Assert.Equal(2, page.PageNumber);
        Assert.Equal(2, page.TotalPages);
        Assert.True(page.HasPreviousPage);
        Assert.False(page.HasNextPage);
    }

    [Fact]
    public void GetPage_ShouldThrow_WhenPageNumberIsGreaterThanTotalPages_AndBehaviorIsThrow()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 5)
            .WithOutOfRangeBehavior(PageOutOfRangeBehavior.Throw)
            .Build();

        void Act()
        {
            paginator.GetPage(3, 3);
        }

        Assert.Throws<ArgumentOutOfRangeException>(Act);
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

    [Fact]
    public void Next_ShouldReturnFollowingPage()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .Build();

        var firstPage = paginator.GetPage(1, 3);
        var nextPage = paginator.Next(firstPage);

        Assert.Equal([4, 5, 6], nextPage.Items);
        Assert.Equal(2, nextPage.PageNumber);
        Assert.Equal(3, nextPage.PageSize);
        Assert.True(nextPage.HasPreviousPage);
        Assert.True(nextPage.HasNextPage);
    }

    [Fact]
    public void Next_ShouldStayOnLastPage_WhenAlreadyOnLastPage()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .Build();

        var lastPage = paginator.GetPage(4, 3);
        var nextPage = paginator.Next(lastPage);

        Assert.Equal([10], nextPage.Items);
        Assert.Equal(4, nextPage.PageNumber);
        Assert.False(nextPage.HasNextPage);
        Assert.True(nextPage.HasPreviousPage);
    }

    [Fact]
    public void Previous_ShouldReturnPreviousPage_OrFirstPage()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .Build();

        var secondPage = paginator.GetPage(2, 3);
        var previousPage = paginator.Previous(secondPage);

        Assert.Equal([1, 2, 3], previousPage.Items);
        Assert.Equal(1, previousPage.PageNumber);

        var firstPage = paginator.GetPage(1, 3);
        var previousFromFirst = paginator.Previous(firstPage);

        Assert.Equal([1, 2, 3], previousFromFirst.Items);
        Assert.Equal(1, previousFromFirst.PageNumber);
    }

    [Fact]
    public void FirstPage_ShouldReturnFirstPage()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .Build();

        var page = paginator.FirstPage(3);

        Assert.Equal([1, 2, 3], page.Items);
        Assert.Equal(1, page.PageNumber);
        Assert.Equal(3, page.PageSize);
    }

    [Fact]
    public void LastPage_ShouldReturnLastPage()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .Build();

        var page = paginator.LastPage(3);

        Assert.Equal([10], page.Items);
        Assert.Equal(4, page.PageNumber);
        Assert.Equal(3, page.PageSize);
        Assert.False(page.HasNextPage);
    }

    [Fact]
    public void LastPage_ShouldReturnEmptyFirstPage_WhenSourceIsEmpty()
    {
        var paginator = new PaginatorBuilder().Build();

        var page = paginator.LastPage(3);

        Assert.Empty(page.Items);
        Assert.Equal(1, page.PageNumber);
        Assert.Equal(0, page.TotalPages);
    }

    [Fact]
    public void GetPage_WithoutPageSize_ShouldUseDefaultPageSize_WhenConfiguredAtInitialization()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .WithDefaultPageSize(4)
            .Build();

        var page = paginator.GetPage(2);

        Assert.Equal([5, 6, 7, 8], page.Items);
        Assert.Equal(4, page.PageSize);
        Assert.Equal(4, paginator.DefaultPageSize);
    }

    [Fact]
    public void FirstAndLastPage_WithoutPageSize_ShouldUseDefaultPageSize()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .WithDefaultPageSize(4)
            .Build();

        var firstPage = paginator.FirstPage();
        var lastPage = paginator.LastPage();

        Assert.Equal([1, 2, 3, 4], firstPage.Items);
        Assert.Equal([9, 10], lastPage.Items);
        Assert.Equal(3, lastPage.PageNumber);
    }

    [Fact]
    public void SetPageSize_ShouldOverrideDefaultPageSize_ForSubsequentCallsWithoutPageSize()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .WithDefaultPageSize(2)
            .Build();

        paginator.SetPageSize(5);

        var page = paginator.GetPage(2);

        Assert.Equal([6, 7, 8, 9, 10], page.Items);
        Assert.Equal(5, page.PageSize);
        Assert.Equal(5, paginator.DefaultPageSize);
    }

    [Fact]
    public void GetPage_WithoutPageSize_ShouldThrow_WhenNoDefaultPageSizeIsConfigured()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .Build();

        void Act()
        {
            paginator.GetPage(1);
        }

        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void FirstPage_WithoutPageSize_ShouldThrow_WhenNoDefaultPageSizeIsConfigured()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .Build();

        void Act()
        {
            paginator.FirstPage();
        }

        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void LastPage_WithoutPageSize_ShouldThrow_WhenNoDefaultPageSizeIsConfigured()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 10)
            .Build();

        void Act()
        {
            paginator.LastPage();
        }

        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void OutOfRangeBehavior_ShouldExposeConfiguredValue()
    {
        var paginator = new PaginatorBuilder()
            .WithRange(1, 5)
            .WithOutOfRangeBehavior(PageOutOfRangeBehavior.ClampToLast)
            .Build();

        Assert.Equal(PageOutOfRangeBehavior.ClampToLast, paginator.OutOfRangeBehavior);
    }
}
