using AwesomeAssertions;
using FrontierSharp.Data.Static;
using Xunit;

namespace FrontierSharp.Tests.Data.Static;

public class ResFileTests {
    [Theory]
    [InlineData("normalfile.txt", "normalfile.txt")]
    [InlineData("app:myappfile.dat", "myappfile.dat")]
    [InlineData("res:myresource.png", "myresource.png")]
    public void Constructor_ShouldSetFilename_Correctly(string inputFilename, string expectedFilename) {
        // Arrange
        var path = "some/relative/path";
        var hash = "abc123";

        // Act
        var resFile = new ResFile(inputFilename, path, hash);

        // Assert
        resFile.Filename.Should().Be(expectedFilename);
    }

    [Fact]
    public void Constructor_ShouldSetRelativePath_Correctly() {
        // Arrange
        var filename = "file.txt";
        var path = "some/relative/path";
        var hash = "abc123";

        // Act
        var resFile = new ResFile(filename, path, hash);

        // Assert
        resFile.RelativePath.Should().Be(path);
    }

    [Fact]
    public void Constructor_ShouldSetHash_Correctly() {
        // Arrange
        var filename = "file.txt";
        var path = "some/relative/path";
        var hash = "def456";

        // Act
        var resFile = new ResFile(filename, path, hash);

        // Assert
        resFile.Hash.Should().Be(hash);
    }

    [Theory]
    [InlineData("app:x", "x")]
    [InlineData("res:x", "x")]
    [InlineData("app:onlyprefix", "onlyprefix")]
    [InlineData("res:onlyprefix", "onlyprefix")]
    public void Constructor_ShouldHandle_EdgeCasesForFilename(string inputFilename, string expectedFilename) {
        // Arrange
        var path = "path";
        var hash = "hash";

        // Act
        var resFile = new ResFile(inputFilename, path, hash);

        // Assert
        resFile.Filename.Should().Be(expectedFilename);
    }
}