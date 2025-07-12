using AwesomeAssertions;
using FrontierSharp.MudIndexer;
using Xunit;

namespace FrontierSharp.Tests.MudIndexer;

public class QueryResultTests {
    [Fact]
    public void DefaultValues_ShouldBeEmpty() {
        var sut = new QueryResult();
        sut.Headers.Should().BeEmpty();
        sut.Rows.Should().BeEmpty();
    }

    [Fact]
    public void CanSetProperties() {
        var sut = new QueryResult {
            Headers = ["foo", "bar"],
            Rows = [[1, "a"], [2, "b"]]
        };
        sut.Headers.Should().BeEquivalentTo("foo", "bar");
        sut.Rows.Should().HaveCount(2);
    }
}