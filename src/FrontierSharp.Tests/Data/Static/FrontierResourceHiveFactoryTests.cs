using System.IO.Abstractions;
using FluentAssertions;
using FrontierSharp.Data;
using FrontierSharp.Data.Static;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.Data.Static {
    public class FrontierResourceHiveFactoryTests {
        private readonly IFileSystem _fileSystem = Substitute.For<IFileSystem>();
        private readonly IOptions<FrontierResourceHiveOptions> _options = Substitute.For<IOptions<FrontierResourceHiveOptions>>();

        [Fact]
        public void Create_ShouldReturn_NewFrontierResourceHive() {
            // Arrange
            var factory = new FrontierResourceHiveFactory(_fileSystem, _options);
            _fileSystem.Directory.Exists(Arg.Any<string>()).Returns(true);
            _fileSystem.Path.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns(ci => System.IO.Path.Combine(ci.ArgAt<string>(0), ci.ArgAt<string>(1)));
            _fileSystem.File.Exists(Arg.Any<string>()).Returns(true);

            // Act
            var hive = factory.Create("rootpath");

            // Assert
            hive.Should().NotBeNull();
            hive.Should().BeOfType<FrontierResourceHive>();
        }
    }
}