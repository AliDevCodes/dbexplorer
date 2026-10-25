using FastDbExplorer.Domain;
using Xunit;

namespace FastDbExplorer.Tests;

/// <summary>When the query switches from OFFSET to keyset paging (Domain rule).</summary>
public class KeysetPagingTests
{
    [Fact]
    public void Offset_paging_is_kept_before_the_threshold()
    {
        Assert.False(KeysetPaging.UseKeyset(true, KeysetPaging.Threshold - 1, true));
    }

    [Fact]
    public void Keyset_paging_starts_at_the_threshold()
    {
        Assert.True(KeysetPaging.UseKeyset(true, KeysetPaging.Threshold, true));
    }

    [Fact]
    public void Keyset_paging_needs_a_usable_key()
    {
        Assert.False(KeysetPaging.UseKeyset(true, KeysetPaging.Threshold * 2, false));
    }

    [Fact]
    public void Keyset_paging_respects_the_query_permission()
    {
        Assert.False(KeysetPaging.UseKeyset(false, KeysetPaging.Threshold * 2, true));
    }
}
