using FluentAssertions;
using StangaNetLib.Core.Pagination;

namespace StangaNetLib.Core.Tests.Pagination;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_ShouldRoundUp()
    {
        var result = new PagedResult<int>([1, 2, 3], page: 1, pageSize: 3, totalCount: 10);
        result.TotalPages.Should().Be(4); // ceil(10/3) = 4
    }

    [Fact]
    public void HasNextPage_OnFirstPage_ShouldBeTrue()
    {
        var result = new PagedResult<int>([1], page: 1, pageSize: 5, totalCount: 20);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_OnLastPage_ShouldBeTrue()
    {
        var result = new PagedResult<int>([1], page: 4, pageSize: 5, totalCount: 20);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Empty_ShouldHaveZeroItems_AndZeroTotalCount()
    {
        var result = PagedResult<string>.Empty();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Map_ShouldProjectItems_PreservingPaginationMetadata()
    {
        var original = new PagedResult<int>([1, 2, 3], page: 2, pageSize: 3, totalCount: 9);
        var mapped = original.Map(i => i.ToString());

        mapped.Items.Should().Equal("1", "2", "3");
        mapped.Page.Should().Be(2);
        mapped.PageSize.Should().Be(3);
        mapped.TotalCount.Should().Be(9);
    }

    [Fact]
    public void PaginationParams_Skip_ShouldBeCorrect()
    {
        var p = new PaginationParams { Page = 3, PageSize = 10 };
        p.Skip.Should().Be(20);
    }

    [Fact]
    public void PaginationParams_PageSize_ShouldBeCappedAtMax()
    {
        var p = new PaginationParams { PageSize = 9999 };
        p.PageSize.Should().Be(PaginationParams.MaxPageSize);
    }
}
