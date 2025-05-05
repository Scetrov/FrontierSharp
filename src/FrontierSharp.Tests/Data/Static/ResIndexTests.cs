using System.IO.Abstractions;
using FluentAssertions;
using FrontierSharp.Data.Static;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.Data.Static;

public class ResIndexTests {
    private readonly IFileSystem _fileSystem;

    public ResIndexTests() {
        _fileSystem = Substitute.For<IFileSystem>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenIndexFileDoesNotExist() {
        // Arrange
        _fileSystem.File.Exists("missingfile.txt").Returns(false);

        // Act
        var act = () => new ResIndex("missingfile.txt", _fileSystem);

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage("Index file not found*");
    }

    [Fact]
    public void Constructor_ShouldParseFiles_Correctly() {
        // Arrange
        var lines = new[] {
            "file1.txt,path1,hash1", "file2.txt,path2,hash2"
        };

        _fileSystem.File.Exists("index.txt").Returns(true);
        _fileSystem.File.ReadAllLines("index.txt").Returns(lines);

        // Act
        var resIndex = new ResIndex("index.txt", _fileSystem);

        // Assert
        resIndex.Files.Should().HaveCount(2);
        resIndex.Files.Should().ContainSingle(x => x.Filename == "file1.txt" && x.RelativePath == "path1" && x.Hash == "hash1");
        resIndex.Files.Should().ContainSingle(x => x.Filename == "file2.txt" && x.RelativePath == "path2" && x.Hash == "hash2");
    }

    [Fact]
    public void FindByFilename_ShouldReturnCorrectFile() {
        // Arrange
        var lines = new[] {
            "target.txt,path,hash"
        };
        _fileSystem.File.Exists("index.txt").Returns(true);
        _fileSystem.File.ReadAllLines("index.txt").Returns(lines);
        var resIndex = new ResIndex("index.txt", _fileSystem);

        // Act
        var result = resIndex.FindByFilename("target.txt");

        // Assert
        result.Should().NotBeNull();
        result.Filename.Should().Be("target.txt");
        result.RelativePath.Should().Be("path");
        result.Hash.Should().Be("hash");
    }
}