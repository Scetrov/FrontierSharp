using System.Text.Json.Nodes;
using FrontierSharp.Mud.Linq;
using AwesomeAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.Mud.Linq;

public class MudTypeRegistryTests {
    private readonly MudTypeRegistry _registry = new();

    [Fact]
    public void AddType_ShouldStoreConverterByType() {
        var converter = Substitute.For<IMudTypeConverter<string>>();
        
        _registry.AddType(converter);
        var result = _registry.GetTypeConverter<string>();

        result.Should().BeSameAs(converter);
    }

    [Fact]
    public void GetTypeConverter_WhenTypeNotRegistered_ShouldReturnGenericConverter() {
        var result = _registry.GetTypeConverter<int>();

        result.Should().BeOfType<GenericMudTypeConverter<int>>();
    }

    [Fact]
    public void GetTypeConverter_WhenConverterIsWrongType_ShouldReturnGenericConverter() {
        var converter = Substitute.For<IMudTypeConverter<int>>();
        _registry.AddType(converter);
        
        var result = _registry.GetTypeConverter<string>();
        result.Should().BeOfType<GenericMudTypeConverter<string>>();
    }

    [UsedImplicitly]
    private class Dummy {
        public string Name { get; set; } = null!;
        public int Age { get; set; }
    }

    [Fact]
    public void GenericMudTypeConverter_ShouldMapPropertiesFromRow() {
        var headings = new JsonNode?[] {
            JsonNode.Parse("[\"Name\", \"Age\"]")
        };

        var converter = new GenericMudTypeConverter<Dummy>().WithHeadings(headings);

        var result = converter.CreateInstance(["Alice", 42]);

        result.Name.Should().Be("Alice");
        result.Age.Should().Be(42);
    }

    [Fact]
    public void GenericMudTypeConverter_ShouldThrow_WhenHeaderIsMissing() {
        var headings = new JsonNode?[] {
            JsonNode.Parse("[\"WrongHeader\"]")
        };

        var converter = new GenericMudTypeConverter<Dummy>().WithHeadings(headings);

        var act = new Func<object>(() => converter.CreateInstance(["Alice"]));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Failed to find the correct index for*Dummy.Name*");
    }

    [Fact]
    public void WithHeadings_ShouldThrow_WhenHeadersMissing() {
        var headings = new JsonNode?[] { null };

        var converter = new GenericMudTypeConverter<Dummy>();

        var act = () => converter.WithHeadings(headings);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Unable to parse the headers, unexpected payload.");
    }
}
