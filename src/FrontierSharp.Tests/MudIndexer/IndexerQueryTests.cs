using AwesomeAssertions;
using FrontierSharp.MudIndexer;
using Xunit;

namespace FrontierSharp.Tests.MudIndexer;

public class IndexerQueryTests {
    [Fact]
    public void CanSetProperties() {
        var sut = new IndexerQuery {
            Address = "0xabc",
            Query = "SELECT * FROM foo"
        };
        sut.Address.Should().Be("0xabc");
        sut.Query.Should().Be("SELECT * FROM foo");
    }
}
