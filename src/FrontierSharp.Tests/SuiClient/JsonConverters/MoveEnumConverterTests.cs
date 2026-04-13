using System.Text.Json;
using System.Text.Json.Serialization;
using FrontierSharp.SuiClient.JsonConverters;
using FrontierSharp.SuiClient.Models;
using AwesomeAssertions;
using Xunit;

namespace FrontierSharp.Tests.SuiClient.JsonConverters;

public class MoveEnumConverterTests {
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public MoveEnumConverterTests() {
        _options.Converters.Add(new MoveEnumConverter<LossType>());
        _options.Converters.Add(new MoveEnumConverter<AssemblyStatus>());
    }

    [Fact]
    public void Read_NumericValue_ParsesLossType() {
        var json = """{"value": 1}""";
        var result = JsonSerializer.Deserialize<LossTypeModel>(json, _options);
        result!.Value.Should().Be(LossType.Ship);
    }

    [Fact]
    public void Read_NumericValue_ParsesStructure() {
        var json = """{"value": 2}""";
        var result = JsonSerializer.Deserialize<LossTypeModel>(json, _options);
        result!.Value.Should().Be(LossType.Structure);
    }

    [Fact]
    public void Read_StringValue_ParsesCaseInsensitive() {
        var json = """{"value": "Ship"}""";
        var result = JsonSerializer.Deserialize<LossTypeModel>(json, _options);
        result!.Value.Should().Be(LossType.Ship);
    }

    [Fact]
    public void Read_StringValue_ParsesUpperCase() {
        var json = """{"value": "STRUCTURE"}""";
        var result = JsonSerializer.Deserialize<LossTypeModel>(json, _options);
        result!.Value.Should().Be(LossType.Structure);
    }

    [Fact]
    public void Read_ObjectVariant_ParsesEnum() {
        var json = """{"value": {"Ship": {}}}""";
        var result = JsonSerializer.Deserialize<LossTypeModel>(json, _options);
        result!.Value.Should().Be(LossType.Ship);
    }

    [Fact]
    public void Read_ObjectVariant_ParsesAssemblyStatus() {
        var json = """{"value": {"Online": {}}}""";
        var result = JsonSerializer.Deserialize<AssemblyStatusModel>(json, _options);
        result!.Value.Should().Be(AssemblyStatus.Online);
    }

    [Fact]
    public void Read_NestedWrapperObject_ParsesAssemblyStatus() {
        var json = """{"value": {"status": {"@variant": "ONLINE"}}}""";
        var result = JsonSerializer.Deserialize<AssemblyStatusModel>(json, _options);
        result!.Value.Should().Be(AssemblyStatus.Online);
    }

    [Fact]
    public void Read_AtVariantObject_ParsesEnum() {
        var json = """{"value": {"@variant": "STRUCTURE"}}""";
        var result = JsonSerializer.Deserialize<LossTypeModel>(json, _options);
        result!.Value.Should().Be(LossType.Structure);
    }

    [Fact]
    public void Read_NumericAssemblyStatus_Parses() {
        var json = """{"value": 2}""";
        var result = JsonSerializer.Deserialize<AssemblyStatusModel>(json, _options);
        result!.Value.Should().Be(AssemblyStatus.Online);
    }

    [Fact]
    public void Read_UnknownString_ThrowsJsonException() {
        var json = """{"value": "NotAVariant"}""";
        var act = () => JsonSerializer.Deserialize<LossTypeModel>(json, _options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_EmptyObject_ThrowsJsonException() {
        var json = """{"value": {}}""";
        var act = () => JsonSerializer.Deserialize<LossTypeModel>(json, _options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Write_ProducesNumericValue() {
        var model = new LossTypeModel { Value = LossType.Structure };
        var json = JsonSerializer.Serialize(model, _options);
        json.Should().Contain("2");
    }

    private class LossTypeModel {
        [JsonConverter(typeof(MoveEnumConverter<LossType>))]
        public LossType Value { get; set; }
    }

    private class AssemblyStatusModel {
        [JsonConverter(typeof(MoveEnumConverter<AssemblyStatus>))]
        public AssemblyStatus Value { get; set; }
    }
}

