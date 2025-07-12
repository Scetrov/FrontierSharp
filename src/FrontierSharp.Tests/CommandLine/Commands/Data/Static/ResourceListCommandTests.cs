using AwesomeAssertions;
using FrontierSharp.CommandLine.Commands.Data.Static;
using FrontierSharp.Data.Static;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Commands.Data.Static;

public class ResourceListCommandTests {
    private readonly IAnsiConsole _ansiConsoleMock = Substitute.For<IAnsiConsole>();
    private readonly IFrontierResourceHiveFactory _hiveFactoryMock = Substitute.For<IFrontierResourceHiveFactory>();
    private readonly IFrontierResourceHive _hiveMock = Substitute.For<IFrontierResourceHive>();
    private readonly ILogger<ResourceListCommand> _loggerMock = Substitute.For<ILogger<ResourceListCommand>>();

    public ResourceListCommandTests() {
        _hiveFactoryMock.Create(Arg.Any<string>()).Returns(_hiveMock);
    }

    [Fact]
    public async Task ExecuteAsync_NoFilesFound_LogsWarningAndReturnsError() {
        // Arrange
        var command = new ResourceListCommand(_loggerMock, _hiveFactoryMock, _ansiConsoleMock);
        var settings = new ResourceListCommand.Settings {
            Root = "testRoot",
            Filter = "nonexistent",
            Type = ".txt"
        };

        _hiveMock.GetIndex().Returns(new ResIndexMock(new List<ResFile>()));
        _hiveMock.GetResIndex().Returns(new ResIndexMock(new List<ResFile>()));

        // Act
        var result = await command.ExecuteAsync(null!, settings);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_FilesMatchFilter_DisplaysResultsAndReturnsSuccess() {
        // Arrange
        var command = new ResourceListCommand(_loggerMock, _hiveFactoryMock, _ansiConsoleMock);
        var settings = new ResourceListCommand.Settings {
            Root = "testRoot",
            Filter = "match",
            Type = ""
        };

        var files = new List<ResFile> {
            new("match_file1.txt", "path1", "hash1"),
            new("match_file2.txt", "path2", "hash2")
        };

        _hiveMock.GetIndex().Returns(new ResIndexMock(files));
        _hiveMock.GetResIndex().Returns(new ResIndexMock(new List<ResFile>()));

        // Act
        var result = await command.ExecuteAsync(null!, settings);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_FilesMatchFilterAndType_DisplaysResultsAndReturnsSuccess() {
        // Arrange
        var command = new ResourceListCommand(_loggerMock, _hiveFactoryMock, _ansiConsoleMock);
        var settings = new ResourceListCommand.Settings {
            Root = "testRoot",
            Filter = "file",
            Type = ".txt"
        };

        var files = new List<ResFile> {
            new("file1.txt", "path1", "hash1"),
            new("file2.txt", "path2", "hash2"),
            new("file3.json", "path3", "hash3")
        };

        _hiveMock.GetIndex().Returns(new ResIndexMock(files));
        _hiveMock.GetResIndex().Returns(new ResIndexMock(new List<ResFile>()));

        // Act
        var result = await command.ExecuteAsync(null!, settings);

        // Assert
        result.Should().Be(0);
    }

    private class ResIndexMock : ResIndex {
        public ResIndexMock(IEnumerable<ResFile> files) : base(null!) {
            Files = files;
        }

        public override ResFile FindByFilename(string filename) {
            return Files.SingleOrDefault(f => f.Filename == filename) ?? throw new FileNotFoundException();
        }
    }
}