using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using FrontierSharp.CommandLine.Commands.Data.Static;
using FrontierSharp.Data.Static;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Spectre.Console;
using Xunit;

namespace FrontierSharp.Tests.CommandLine.Commands.Data.Static;

public class ResourceUnpickleCommandTests {
    private readonly ILogger<ResourceListCommand> _loggerMock = Substitute.For<ILogger<ResourceListCommand>>();
    private readonly IFrontierResourceHiveFactory _hiveFactoryMock = Substitute.For<IFrontierResourceHiveFactory>();
    private readonly IAnsiConsole _ansiConsoleMock = Substitute.For<IAnsiConsole>();
    private readonly IFrontierResourceHive _hiveMock = Substitute.For<IFrontierResourceHive>();
    private readonly MockFileSystem _filesystemMock = new();

    public ResourceUnpickleCommandTests() {
        _hiveFactoryMock.Create(Arg.Any<string>()).Returns(_hiveMock);
    }

    [Fact]
    public async Task ExecuteAsync_NoFilesFound_LogsWarningAndReturnsError() {
        // Arrange
        var command = new ResourceUnpickleCommand(_loggerMock, _hiveFactoryMock, _filesystemMock, _ansiConsoleMock);
        var settings = new ResourceUnpickleCommand.Settings {
            Root = "testRoot",
            Filename = "nonexistent.file",
            RelativePath = null
        };

        _hiveMock.GetIndex().Returns(new ResIndexMock(new List<ResFile>()));
        _hiveMock.GetResIndex().Returns(new ResIndexMock(new List<ResFile>()));

        // Act
        var result = await command.ExecuteAsync(null!, settings);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleFilesFound_LogsErrorAndReturnsError() {
        // Arrange
        var command = new ResourceUnpickleCommand(_loggerMock, _hiveFactoryMock, _filesystemMock, _ansiConsoleMock);
        var settings = new ResourceUnpickleCommand.Settings {
            Root = "testRoot",
            Filename = "duplicate.file",
            RelativePath = null
        };

        var files = new List<ResFile> {
            new("duplicate.file", "path1", "hash1"),
            new("duplicate.file", "path2", "hash2"),
            new("duplicate.file", "path3", "hash3")
        };

        _hiveMock.GetIndex().Returns(new ResIndexMock(files));
        _hiveMock.GetResIndex().Returns(new ResIndexMock(new List<ResFile>()));

        // Act
        var result = await command.ExecuteAsync(null!, settings);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ExecuteAsync_ValidFile_UnpicklesAndOutputsJson() {
        // Arrange
        var fileSystem = new MockFileSystem();
        var base64 = "gASVEgAAAAAAAAB9lIwDa2V5lIwFdmFsdWWUcy4=";
        var bytes = Convert.FromBase64String(base64);
        fileSystem.AddFile("absolute/path", new MockFileData(bytes));
        var command = new ResourceUnpickleCommand(_loggerMock, _hiveFactoryMock, fileSystem, _ansiConsoleMock);
        var settings = new ResourceUnpickleCommand.Settings {
            Root = "testRoot",
            Filename = "valid.file",
            Output = "output.json",
            OutputFormat = ResourceUnpickleCommand.Settings.OutputFormatOption.Json,
            RelativePath = null
        };

        var file = new ResFile("valid.file", "relative/path", "hash");
        _hiveMock.GetIndex().Returns(new ResIndexMock(new List<ResFile> { file }));
        _hiveMock.GetResIndex().Returns(new ResIndexMock(new List<ResFile>()));
        _hiveMock.ResolvePath("relative/path").Returns("absolute/path");

        var unpickledContent = new { key = "value" };

        // Act
        var result = await command.ExecuteAsync(CommandContextHelper.Create(), settings);

        // Assert
        Assert.Equal(0, result);
        var expectedJson = JsonSerializer.Serialize(unpickledContent, new JsonSerializerOptions { WriteIndented = true });
        Assert.Equal(expectedJson, fileSystem.File.ReadAllText("output.json"));
    }

    private class ResIndexMock(IEnumerable<ResFile> files) : ResIndex(files) {
        public override ResFile FindByFilename(string filename) {
            return Files.SingleOrDefault(f => f.Filename == filename) ?? throw new FileNotFoundException();
        }
    }
}
