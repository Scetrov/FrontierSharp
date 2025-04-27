using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using FrontierSharp.Data;
using FrontierSharp.Data.Static;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.Data.Static {
    public class FrontierResourceHiveTests {
        private readonly IFileSystem _fileSystem;
        private readonly IOptions<FrontierResourceHiveOptions> _options;

        public FrontierResourceHiveTests() {
            _fileSystem = Substitute.For<IFileSystem>();
            var optionsValue = new FrontierResourceHiveOptions {
                ServerName = "TestServer",
                ResFileIndexKey = "ResIndexKey",
                LocalisationMainKey = "LocalisationMainKey",
                LocalisationEnglishPatten = "localisation_{0}.dat"
            };
            _options = Substitute.For<IOptions<FrontierResourceHiveOptions>>();
            _options.Value.Returns(optionsValue);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenRootDirectoryDoesNotExist() {
            // Arrange
            _fileSystem.Directory.Exists(Arg.Any<string>()).Returns(false);

            // Act
            var act = () => new FrontierResourceHive("invalid_root", _fileSystem, _options);

            // Assert
            act.Should().Throw<DirectoryNotFoundException>()
                .WithMessage("Directory not found: *");
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenResFilesDirectoryDoesNotExist() {
            // Arrange
            _fileSystem.Directory.Exists("root").Returns(true);
            _fileSystem.Path.Combine("root", "ResFiles").Returns("root/ResFiles");
            _fileSystem.Directory.Exists("root/ResFiles").Returns(false);

            // Act
            var act = () => new FrontierResourceHive("root", _fileSystem, _options);

            // Assert
            act.Should().Throw<DirectoryNotFoundException>()
                .WithMessage("ResFiles not found in *");
        }

        [Fact]
        public void GetIndex_ShouldThrow_WhenIndexFileDoesNotExist() {
            // Arrange
            _fileSystem.Directory.Exists(Arg.Any<string>()).Returns(true);
            _fileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns(ci => Path.Combine(ci.ArgAt<string>(0), ci.ArgAt<string>(1)));
            _fileSystem.File.Exists(Arg.Any<string>()).Returns(false);

            var hive = new FrontierResourceHive("root", _fileSystem, _options);

            // Act
            var act = () => hive.GetIndex();

            // Assert
            act.Should().Throw<FileNotFoundException>()
                .WithMessage("Index not found*");
        }

        [Fact]
        public void GetIndex_ShouldReturn_ResIndex() {
            // Arrange
            _fileSystem.Directory.Exists(Arg.Any<string>()).Returns(true);
            _fileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns(ci => Path.Combine(ci.ArgAt<string>(0), ci.ArgAt<string>(1)));
            _fileSystem.File.Exists(Arg.Any<string>()).Returns(true);

            var hive = new FrontierResourceHive("root", _fileSystem, _options);

            // Act
            var index = hive.GetIndex();

            // Assert
            index.Should().NotBeNull();
            index.Should().BeOfType<ResIndex>();
        }

        [Fact]
        public void ResolvePath_ShouldThrow_WhenFileDoesNotExist() {
            // Arrange
            _fileSystem.Directory.Exists(Arg.Any<string>()).Returns(true);
            _fileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns(ci => Path.Combine(ci.ArgAt<string>(0), ci.ArgAt<string>(1)));
            _fileSystem.File.Exists(Arg.Any<string>()).Returns(false);

            var hive = new FrontierResourceHive("root", _fileSystem, _options);

            // Act
            var act = () => hive.ResolvePath("somefile.txt");

            // Assert
            act.Should().Throw<FileNotFoundException>()
                .WithMessage("Resource file not found*");
        }

        [Fact]
        public void ResolvePath_ShouldReturn_ResolvedPath() {
            // Arrange
            _fileSystem.Directory.Exists(Arg.Any<string>()).Returns(true);
            _fileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns(ci => Path.Combine(ci.ArgAt<string>(0), ci.ArgAt<string>(1)));
            _fileSystem.File.Exists(Arg.Any<string>()).Returns(true);

            var hive = new FrontierResourceHive("root", _fileSystem, _options);

            // Act
            var path = hive.ResolvePath("somefile.txt");

            // Assert
            path.Should().EndWith(Path.Combine("root", "ResFiles", "somefile.txt"));
        }

        [Fact]
        public void GetLocalisationByCulture_ShouldFormatCorrectly() {
            // Arrange
            _fileSystem.Directory.Exists(Arg.Any<string>()).Returns(true);
            _fileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns(ci => Path.Combine(ci.ArgAt<string>(0), ci.ArgAt<string>(1)));
            _fileSystem.File.Exists(Arg.Any<string>()).Returns(true);

            var hive = new FrontierResourceHive("root", _fileSystem, _options);

            // Act
            var method = typeof(FrontierResourceHive).GetMethod("GetLocalisationByCulture",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method?.Invoke(hive, [
                "en-us"
            ]);

            // Assert
            result.Should().Be("localisation_en-us.dat");
        }

        [Fact]
        public void GetNames_ShouldReturn_Stream() {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile("/root/index_stillness.txt", new MockFileData("/resfileindex.txt,resfileindex.txt,someHash"));
            mockFileSystem.AddFile("/root/ResFiles/resfileindex.txt", new MockFileData("/localizationfsd/localization_fsd_main.pickle,/localizationfsd/localization_fsd_main.pickle,someOtherHash"));
            mockFileSystem.AddFile("/root/ResFiles/localizationfsd/localization_fsd_main.pickle", new MockFileData("content"));
            mockFileSystem.AddFile("/localizationfsd/localization_fsd_main.pickle", new MockFileData("content"));
            
            var options = Substitute.For<IOptions<FrontierResourceHiveOptions>>();
            options.Value.Returns(new FrontierResourceHiveOptions());

            var hive = new FrontierResourceHive("/root", mockFileSystem, options);

            // Act
            var result = hive.GetNames();

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void GetTranslations_ShouldReturn_Stream() {
            // Arrange
            var filePath = "localizationfsd/localization_fsd_en-us.pickle";
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile("/root/index_stillness.txt", new MockFileData("/resfileindex.txt,resfileindex.txt,someHash"));
            mockFileSystem.AddFile("/root/ResFiles/resfileindex.txt", new MockFileData($"/{filePath},{filePath},someOtherHash"));
            mockFileSystem.AddFile($"/root/ResFiles/{filePath}", new MockFileData("content"));
            mockFileSystem.AddFile($"/{filePath}", new MockFileData("content"));

            var options = Substitute.For<IOptions<FrontierResourceHiveOptions>>();
            options.Value.Returns(new FrontierResourceHiveOptions());

            var hive = new FrontierResourceHive("/root", mockFileSystem, options);

            // Act
            var result = hive.GetTranslations();

            // Assert
            result.Should().NotBeNull();
        }
    }
}