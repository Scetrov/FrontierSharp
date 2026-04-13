using System.Text.Json;
using AwesomeAssertions;
using FluentResults;
using FrontierSharp.SuiClient;
using FrontierSharp.SuiClient.GraphQl;
using FrontierSharp.SuiClient.Models;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FrontierSharp.Tests.SuiClient;

public class WorldClientTests {
    private const string PackageAddress = "0xdeadbeef";

    private readonly ISuiGraphQlClient _graphQlClient = Substitute.For<ISuiGraphQlClient>();
    private readonly MockLogger<WorldClient> _logger = Substitute.For<MockLogger<WorldClient>>();
    private readonly IOptions<SuiClientOptions> _options = Substitute.For<IOptions<SuiClientOptions>>();
    private readonly WorldClient _worldClient;

    public WorldClientTests() {
        _options.Value.Returns(new SuiClientOptions {
            WorldPackageAddress = PackageAddress
        });
        _worldClient = new WorldClient(_graphQlClient, _options, _logger);
    }

    private static ObjectsQueryData BuildObjectsResponse(params JsonElement[] contentJsonElements) {
        return BuildObjectsResponse(false, null, contentJsonElements);
    }

    private static ObjectsQueryData BuildObjectsResponse(bool hasNextPage, string? endCursor,
        params JsonElement[] contentJsonElements) {
        var nodes = contentJsonElements.Select((json, i) => new ObjectNode {
            Address = $"0xobj{i}",
            AsMoveObject = new MoveObjectData {
                Contents = new MoveContents { Json = json }
            }
        }).ToList();

        return new ObjectsQueryData {
            Objects = new ObjectConnection {
                Nodes = nodes,
                PageInfo = new PageInfo {
                    HasNextPage = hasNextPage,
                    EndCursor = endCursor
                }
            }
        };
    }

    private static JsonElement ParseJson(string json) {
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private static JsonElement CreateKillmailJson(ulong itemId) {
        return ParseJson($$"""
            {
                "id": "0xkillmail{{itemId}}",
                "key": { "item_id": "{{itemId}}", "tenant": "t" },
                "killer_id": { "item_id": "100", "tenant": "t" },
                "victim_id": { "item_id": "200", "tenant": "t" },
                "reported_by_character_id": { "item_id": "300", "tenant": "t" },
                "kill_timestamp": "0",
                "loss_type": 1,
                "solar_system_id": { "item_id": "400", "tenant": "t" }
            }
            """);
    }

    private static JsonElement CreateCharacterJson(ulong itemId) {
        return ParseJson($$"""
            {
                "key": { "item_id": "{{itemId}}", "tenant": "t" },
                "tribe_id": 42,
                "character_address": "0xchar{{itemId}}",
                "owner_cap_id": "0xcap{{itemId}}",
                "metadata": null
            }
            """);
    }

    private static JsonElement CreateAssemblyJson(ulong itemId) {
        return ParseJson($$"""
            {
                "key": { "item_id": "{{itemId}}", "tenant": "t" },
                "type_id": "{{itemId}}",
                "owner_cap_id": "0xcap{{itemId}}",
                "status": 2,
                "location": "0xloc{{itemId}}",
                "energy_source_id": null
            }
            """);
    }

    #region Killmail Tests

    [Fact]
    public async Task GetKillmailsAsync_ReturnsDeserializedKillmails() {
        var killmailJson = ParseJson("""
            {
                "id": "0xkillmail",
                "key": { "item_id": "100", "tenant": "0xtenant" },
                "killer_id": { "item_id": "200", "tenant": "0xtenant" },
                "victim_id": { "item_id": "300", "tenant": "0xtenant" },
                "reported_by_character_id": { "item_id": "400", "tenant": "0xtenant" },
                "kill_timestamp": "1700000000",
                "loss_type": 1,
                "solar_system_id": { "item_id": "500", "tenant": "0xtenant" }
            }
            """);

        var response = BuildObjectsResponse(killmailJson);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetKillmailsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(1);

        var km = result.Value.Data[0];
        km.Id.Should().Be("0xkillmail");
        km.Key.ItemId.Should().Be(100UL);
        km.Key.Tenant.Should().Be("0xtenant");
        km.KillerId.Should().Be(200UL);
        km.VictimId.Should().Be(300UL);
        km.ReportedByCharacterId.Should().Be(400UL);
        km.KillTimestamp.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1700000000));
        km.LossType.Should().Be(LossType.Ship);
        km.SolarSystemId.Should().Be(500UL);
    }

    [Fact]
    public async Task GetKillmailsAsync_DoesNotApplyClientSideFiltering() {
        var km1 = ParseJson("""
            {
                "key": { "item_id": "1", "tenant": "t" },
                "killer_id": { "item_id": "100", "tenant": "t" }, "victim_id": { "item_id": "200", "tenant": "t" },
                "reported_by_character_id": { "item_id": "300", "tenant": "t" }, "kill_timestamp": "0",
                "loss_type": 1, "solar_system_id": { "item_id": "0", "tenant": "t" }
            }
            """);
        var km2 = ParseJson("""
            {
                "key": { "item_id": "2", "tenant": "t" },
                "killer_id": { "item_id": "100", "tenant": "t" }, "victim_id": { "item_id": "999", "tenant": "t" },
                "reported_by_character_id": { "item_id": "300", "tenant": "t" }, "kill_timestamp": "0",
                "loss_type": 1, "solar_system_id": { "item_id": "0", "tenant": "t" }
            }
            """);

        var response = BuildObjectsResponse(km1, km2);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetKillmailsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetKillmailsAsync_Pagination_PropagatesPageInfo() {
        var km = ParseJson("""
            {
                "key": { "item_id": "1", "tenant": "t" },
                "killer_id": { "item_id": "100", "tenant": "t" }, "victim_id": { "item_id": "200", "tenant": "t" },
                "reported_by_character_id": { "item_id": "300", "tenant": "t" }, "kill_timestamp": "0",
                "loss_type": 1, "solar_system_id": { "item_id": "0", "tenant": "t" }
            }
            """);

        var response = BuildObjectsResponse(true, "cursor123", km);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetKillmailsAsync(first: 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.HasNextPage.Should().BeTrue();
        result.Value.EndCursor.Should().Be(new Cursor("cursor123"));
    }

    [Fact]
    public async Task GetAllKillmailsAsync_ReturnsAllPages_WhenMultiplePagesExist() {
        var firstPage = BuildObjectsResponse(true, "cursor-1", CreateKillmailJson(1));
        var secondPage = BuildObjectsResponse(false, null, CreateKillmailJson(2));

        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(callInfo => {
            var variables = callInfo.ArgAt<Dictionary<string, object?>>(1);
            var after = variables["after"] as string;

            return after switch {
                null => Task.FromResult(Result.Ok(firstPage)),
                "cursor-1" => Task.FromResult(Result.Ok(secondPage)),
                _ => throw new InvalidOperationException($"Unexpected cursor {after}")
            };
        });

        var result = await _worldClient.GetAllKillmailsAsync(first: 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(killmail => killmail.Key.ItemId).Should().Equal(1UL, 2UL);
    }

    [Fact]
    public async Task GetAllKillmailsAsync_ReturnsFailure_WhenSubsequentPageFails() {
        var firstPage = BuildObjectsResponse(true, "cursor-1", CreateKillmailJson(1));

        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(callInfo => {
            var variables = callInfo.ArgAt<Dictionary<string, object?>>(1);
            var after = variables["after"] as string;

            return after switch {
                null => Task.FromResult(Result.Ok(firstPage)),
                "cursor-1" => Task.FromResult(Result.Fail<ObjectsQueryData>("GraphQL error")),
                _ => throw new InvalidOperationException($"Unexpected cursor {after}")
            };
        });

        var result = await _worldClient.GetAllKillmailsAsync(first: 1);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(error => error.Message.Contains("GraphQL error"));
    }

    [Fact]
    public async Task GetAllKillmailsAsync_ReturnsFailure_WhenNextPageCursorIsMissing() {
        var response = BuildObjectsResponse(true, null, CreateKillmailJson(1));
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetAllKillmailsAsync(first: 1);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(error => error.Message.Contains("did not provide an end cursor"));
    }

    [Fact]
    public async Task GetKillmailsAsync_PassesCursorAsAfterVariable() {
        var response = BuildObjectsResponse();
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var after = new Cursor("cursor123");

        var result = await _worldClient.GetKillmailsAsync(first: 10, after: after);

        result.IsSuccess.Should().BeTrue();

        await _graphQlClient.Received(1).QueryAsync<ObjectsQueryData>(
            WorldQueries.GetObjectsByType,
            Arg.Is<Dictionary<string, object?>>(v =>
                v["type"]!.ToString() == $"{PackageAddress}::killmail::Killmail" &&
                Equals(v["first"], 10) &&
                Equals(v["after"], "cursor123")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetKillmailsAsync_GraphQlFailure_PropagatesError() {
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Fail<ObjectsQueryData>("GraphQL error"));

        var result = await _worldClient.GetKillmailsAsync();

        result.IsFailed.Should().BeTrue();
        result.Errors[0].Message.Should().Contain("GraphQL error");
    }

    [Fact]
    public async Task GetKillmailsAsync_PassesCorrectMoveType() {
        var response = BuildObjectsResponse();
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        await _worldClient.GetKillmailsAsync();

        await _graphQlClient.Received(1).QueryAsync<ObjectsQueryData>(
            WorldQueries.GetObjectsByType,
            Arg.Is<Dictionary<string, object?>>(v =>
                v["type"]!.ToString() == $"{PackageAddress}::killmail::Killmail"),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Character Tests

    [Fact]
    public async Task GetCharactersAsync_ReturnsDeserializedCharacters() {
        var charJson = ParseJson("""
            {
                "key": { "item_id": "10", "tenant": "0xtenant" },
                "tribe_id": 42,
                "character_address": "0xchar_addr",
                "owner_cap_id": "0xcap_id",
                "metadata": {
                    "assembly_id": "0xassembly",
                    "name": "EventHorizon",
                    "description": "",
                    "url": "https://example.test"
                }
            }
            """);

        var response = BuildObjectsResponse(charJson);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetCharactersAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(1);

        var ch = result.Value.Data[0];
        ch.Key.ItemId.Should().Be(10UL);
        ch.TribeId.Should().Be(42U);
        ch.CharacterAddress.Should().Be("0xchar_addr");
        ch.OwnerCapId.Should().Be("0xcap_id");
        ch.Metadata.Should().NotBeNull();
        ch.Metadata!.AssemblyId.Should().Be("0xassembly");
        ch.Metadata.Name.Should().Be("EventHorizon");
        ch.Metadata.Description.Should().BeEmpty();
        ch.Metadata.Url.Should().Be("https://example.test");
    }

    [Fact]
    public async Task GetCharactersAsync_NullMetadata_HandledCorrectly() {
        var charJson = ParseJson("""
            {
                "key": { "item_id": "10", "tenant": "0xtenant" },
                "tribe_id": 42,
                "character_address": "0xchar_addr",
                "owner_cap_id": "0xcap_id",
                "metadata": null
            }
            """);

        var response = BuildObjectsResponse(charJson);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetCharactersAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data[0].Metadata.Should().BeNull();
    }

    [Fact]
    public async Task GetCharactersAsync_StringMetadata_PreservesLegacyRawValue() {
        var charJson = ParseJson("""
            {
                "key": { "item_id": "10", "tenant": "0xtenant" },
                "tribe_id": 42,
                "character_address": "0xchar_addr",
                "owner_cap_id": "0xcap_id",
                "metadata": "some_meta"
            }
            """);

        var response = BuildObjectsResponse(charJson);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetCharactersAsync();

        result.IsSuccess.Should().BeTrue();
        var metadata = result.Value.Data[0].Metadata;
        metadata.Should().NotBeNull();
        metadata!.RawValue.Should().Be("some_meta");
        metadata.AssemblyId.Should().BeNull();
    }

    [Fact]
    public async Task GetCharactersAsync_ObjectMetadata_HandledCorrectly() {
        var charJson = ParseJson("""
            {
                "id": "0x0000066c06ea7fe23accf0264661256d0013e648c4e1087543439282a7ccd0a6",
                "key": { "item_id": "2112087484", "tenant": "stillness" },
                "tribe_id": 1000167,
                "character_address": "0xcaefec396249d9852daea51285a0caf8917bbab9373d37d8bb78d452afd36626",
                "metadata": {
                    "assembly_id": "0x0000066c06ea7fe23accf0264661256d0013e648c4e1087543439282a7ccd0a6",
                    "name": "EventHorizon",
                    "description": "",
                    "url": ""
                },
                "owner_cap_id": "0x5799295378c92c2cf3f45c3d6f73169b99592a034cd77b39064371472c8c0f3f"
            }
            """);

        var response = BuildObjectsResponse(charJson);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetCharactersAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(1);

        var metadata = result.Value.Data[0].Metadata;
        metadata.Should().NotBeNull();
        metadata.AssemblyId.Should().Be("0x0000066c06ea7fe23accf0264661256d0013e648c4e1087543439282a7ccd0a6");
        metadata.Name.Should().Be("EventHorizon");
        metadata.Description.Should().BeEmpty();
        metadata.Url.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCharactersAsync_DoesNotApplyClientSideFiltering() {
        var ch1 = ParseJson("""
            {
                "key": { "item_id": "1", "tenant": "t" },
                "tribe_id": 1, "character_address": "0xAlice",
                "owner_cap_id": "0xcap1", "metadata": null
            }
            """);
        var ch2 = ParseJson("""
            {
                "key": { "item_id": "2", "tenant": "t" },
                "tribe_id": 2, "character_address": "0xBob",
                "owner_cap_id": "0xcap2", "metadata": null
            }
            """);

        var response = BuildObjectsResponse(ch1, ch2);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetCharactersAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCharactersAsync_PassesCorrectMoveType() {
        var response = BuildObjectsResponse();
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        await _worldClient.GetCharactersAsync();

        await _graphQlClient.Received(1).QueryAsync<ObjectsQueryData>(
            WorldQueries.GetObjectsByType,
            Arg.Is<Dictionary<string, object?>>(v =>
                v["type"]!.ToString() == $"{PackageAddress}::character::Character"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllCharactersAsync_ReturnsAllPages_WhenMultiplePagesExist() {
        var firstPage = BuildObjectsResponse(true, "cursor-1", CreateCharacterJson(1));
        var secondPage = BuildObjectsResponse(false, null, CreateCharacterJson(2));

        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(callInfo => {
            var variables = callInfo.ArgAt<Dictionary<string, object?>>(1);
            var after = variables["after"] as string;

            return after switch {
                null => Task.FromResult(Result.Ok(firstPage)),
                "cursor-1" => Task.FromResult(Result.Ok(secondPage)),
                _ => throw new InvalidOperationException($"Unexpected cursor {after}")
            };
        });

        var result = await _worldClient.GetAllCharactersAsync(first: 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(character => character.Key.ItemId).Should().Equal(1UL, 2UL);
    }

    #endregion

    #region Assembly Tests

    [Fact]
    public async Task GetAssembliesAsync_ReturnsDeserializedAssemblies() {
        var asmJson = ParseJson("""
            {
                "key": { "item_id": "50", "tenant": "0xtenant" },
                "type_id": "999",
                "owner_cap_id": "0xowner_cap",
                "status": 2,
                "location": "0xhashed_location",
                "energy_source_id": "0xenergy"
            }
            """);

        var response = BuildObjectsResponse(asmJson);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetAssembliesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(1);

        var asm = result.Value.Data[0];
        asm.Key.ItemId.Should().Be(50UL);
        asm.TypeId.Should().Be(999UL);
        asm.OwnerCapId.Should().Be("0xowner_cap");
        asm.Status.Should().Be(AssemblyStatus.Online);
        asm.Location.Should().Be("0xhashed_location");
        asm.EnergySourceId.Should().Be("0xenergy");
    }

    [Fact]
    public async Task GetAssembliesAsync_NullEnergySourceId_HandledCorrectly() {
        var asmJson = ParseJson("""
            {
                "key": { "item_id": "50", "tenant": "0xtenant" },
                "type_id": "999",
                "owner_cap_id": "0xowner_cap",
                "status": 0,
                "location": "0xloc",
                "energy_source_id": null
            }
            """);

        var response = BuildObjectsResponse(asmJson);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetAssembliesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data[0].EnergySourceId.Should().BeNull();
        result.Value.Data[0].Status.Should().Be(AssemblyStatus.Unanchored);
    }

    [Fact]
    public async Task GetAssembliesAsync_LivePayloadShape_Parses() {
        var asmJson = ParseJson("""
            {
                "id": "0x000414dd6ad31ff7cf37ecf299e704670d90a0e4bf8d514352f568d323435018",
                "key": { "item_id": "1000004997350", "tenant": "stillness" },
                "owner_cap_id": "0x428b1410b727d6632b85c08d71629a68f9831ef412fb076448dbe8d71c1a8694",
                "type_id": "90184",
                "status": { "status": { "@variant": "ONLINE" } },
                "location": { "location_hash": "wtTe4ujlqU+yJxH95e+wVb8tV+3EFtMlR4NHxWfX54c=" },
                "energy_source_id": "0x51839ec6657133e2ca55c02e50c27006e6015a18ac3252d5520422406cadbb20",
                "metadata": {
                    "assembly_id": "0x000414dd6ad31ff7cf37ecf299e704670d90a0e4bf8d514352f568d323435018",
                    "name": "",
                    "description": "",
                    "url": ""
                }
            }
            """);

        var response = BuildObjectsResponse(asmJson);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetAssembliesAsync(first: 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(1);

        var assembly = result.Value.Data[0];
        assembly.Key.ItemId.Should().Be(1000004997350UL);
        assembly.TypeId.Should().Be(90184UL);
        assembly.OwnerCapId.Should().Be("0x428b1410b727d6632b85c08d71629a68f9831ef412fb076448dbe8d71c1a8694");
        assembly.Status.Should().Be(AssemblyStatus.Online);
        assembly.Location.Should().Be("wtTe4ujlqU+yJxH95e+wVb8tV+3EFtMlR4NHxWfX54c=");
        assembly.EnergySourceId.Should().Be("0x51839ec6657133e2ca55c02e50c27006e6015a18ac3252d5520422406cadbb20");
    }

    [Fact]
    public async Task GetAssembliesAsync_DoesNotApplyClientSideFiltering() {
        var asm1 = ParseJson("""
            {
                "key": { "item_id": "1", "tenant": "t" },
                "type_id": "100", "owner_cap_id": "0xcap1",
                "status": 1, "location": "0xloc", "energy_source_id": null
            }
            """);
        var asm2 = ParseJson("""
            {
                "key": { "item_id": "2", "tenant": "t" },
                "type_id": "200", "owner_cap_id": "0xcap2",
                "status": 1, "location": "0xloc", "energy_source_id": null
            }
            """);

        var response = BuildObjectsResponse(asm1, asm2);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetAssembliesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAssembliesAsync_PassesCorrectMoveType() {
        var response = BuildObjectsResponse();
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        await _worldClient.GetAssembliesAsync();

        await _graphQlClient.Received(1).QueryAsync<ObjectsQueryData>(
            WorldQueries.GetObjectsByType,
            Arg.Is<Dictionary<string, object?>>(v =>
                v["type"]!.ToString() == $"{PackageAddress}::assembly::Assembly"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAssembliesAsync_ReturnsAllPages_WhenMultiplePagesExist() {
        var firstPage = BuildObjectsResponse(true, "cursor-1", CreateAssemblyJson(1));
        var secondPage = BuildObjectsResponse(false, null, CreateAssemblyJson(2));

        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(callInfo => {
            var variables = callInfo.ArgAt<Dictionary<string, object?>>(1);
            var after = variables["after"] as string;

            return after switch {
                null => Task.FromResult(Result.Ok(firstPage)),
                "cursor-1" => Task.FromResult(Result.Ok(secondPage)),
                _ => throw new InvalidOperationException($"Unexpected cursor {after}")
            };
        });

        var result = await _worldClient.GetAllAssembliesAsync(first: 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(assembly => assembly.Key.ItemId).Should().Equal(1UL, 2UL);
    }


    #endregion

    #region Edge Cases

    [Fact]
    public async Task QuerySkipsNodesWithNullMoveContent() {
        var validKm = ParseJson("""
            {
                "key": { "item_id": "1", "tenant": "t" },
                "killer_id": { "item_id": "100", "tenant": "t" }, "victim_id": { "item_id": "200", "tenant": "t" },
                "reported_by_character_id": { "item_id": "300", "tenant": "t" }, "kill_timestamp": "0",
                "loss_type": 1, "solar_system_id": { "item_id": "0", "tenant": "t" }
            }
            """);

        var data = new ObjectsQueryData {
            Objects = new ObjectConnection {
                Nodes = [
                    new ObjectNode {
                        Address = "0xgood",
                        AsMoveObject = new MoveObjectData {
                            Contents = new MoveContents { Json = validKm }
                        }
                    },
                    new ObjectNode {
                        Address = "0xnull_content",
                        AsMoveObject = null
                    },
                    new ObjectNode {
                        Address = "0xnull_contents",
                        AsMoveObject = new MoveObjectData { Contents = null }
                    }
                ],
                PageInfo = new PageInfo { HasNextPage = false }
            }
        };

        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(data));

        var result = await _worldClient.GetKillmailsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetKillmailsAsync_LossTypeAsObjectVariant_Parses() {
        var killmailJson = ParseJson("""
            {
                "key": { "item_id": "1", "tenant": "t" },
                "killer_id": { "item_id": "100", "tenant": "t" }, "victim_id": { "item_id": "200", "tenant": "t" },
                "reported_by_character_id": { "item_id": "300", "tenant": "t" }, "kill_timestamp": "0",
                "loss_type": {"Structure": {}},
                "solar_system_id": { "item_id": "0", "tenant": "t" }
            }
            """);

        var response = BuildObjectsResponse(killmailJson);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetKillmailsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data[0].LossType.Should().Be(LossType.Structure);
    }

    [Fact]
    public async Task GetKillmailsAsync_LivePayloadShape_Parses() {
        var killmailJson = ParseJson("""
            {
                "id": "0x0011b762d28a065fbcfe79bb2b4a277bc3d43806aff00f94cf48f85ccca8ab74",
                "key": { "item_id": "9300", "tenant": "stillness" },
                "killer_id": { "item_id": "2112085059", "tenant": "stillness" },
                "victim_id": { "item_id": "2112082572", "tenant": "stillness" },
                "reported_by_character_id": { "item_id": "2112081168", "tenant": "stillness" },
                "kill_timestamp": "1775418259",
                "loss_type": { "@variant": "STRUCTURE" },
                "solar_system_id": { "item_id": "30016335", "tenant": "stillness" }
            }
            """);

        var response = BuildObjectsResponse(killmailJson);
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetKillmailsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().HaveCount(1);
        result.Value.Data[0].Id.Should().Be("0x0011b762d28a065fbcfe79bb2b4a277bc3d43806aff00f94cf48f85ccca8ab74");
        result.Value.Data[0].LossType.Should().Be(LossType.Structure);
        result.Value.Data[0].SolarSystemId.Should().Be(30016335UL);
        result.Value.Data[0].KillTimestamp.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1775418259));
    }

    [Fact]
    public async Task GetKillmailsAsync_InvalidNodeAlongsideValidNode_ReturnsFailure() {
        var validKillmailJson = ParseJson("""
            {
                "key": { "item_id": "1", "tenant": "t" },
                "killer_id": { "item_id": "100", "tenant": "t" }, "victim_id": { "item_id": "200", "tenant": "t" },
                "reported_by_character_id": { "item_id": "300", "tenant": "t" }, "kill_timestamp": "0",
                "loss_type": 1, "solar_system_id": { "item_id": "0", "tenant": "t" }
            }
            """);
        var invalidKillmailJson = ParseJson("""
            {
                "key": { "item_id": { "nested": "not-a-u64" }, "tenant": "t" },
                "killer_id": { "item_id": "100", "tenant": "t" }, "victim_id": { "item_id": "200", "tenant": "t" },
                "reported_by_character_id": { "item_id": "300", "tenant": "t" }, "kill_timestamp": "0",
                "loss_type": 1, "solar_system_id": { "item_id": "0", "tenant": "t" }
            }
            """);

        var response = new ObjectsQueryData {
            Objects = new ObjectConnection {
                Nodes = [
                    new ObjectNode {
                        Address = "0xvalid",
                        AsMoveObject = new MoveObjectData {
                            Contents = new MoveContents { Json = validKillmailJson }
                        }
                    },
                    new ObjectNode {
                        Address = "0xinvalid",
                        AsMoveObject = new MoveObjectData {
                            Contents = new MoveContents { Json = invalidKillmailJson }
                        }
                    }
                ],
                PageInfo = new PageInfo { HasNextPage = false }
            }
        };

        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetKillmailsAsync();

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(error => error.Message.Contains("0xinvalid"));
        result.Errors.Should().Contain(error => error.Message.Contains("Cannot convert StartObject to UInt64"));
    }

    [Fact]
    public async Task GetKillmailsAsync_AllInvalidNodes_ReturnsFailureInsteadOfEmptySuccess() {
        var invalidKillmailJson = ParseJson("""
            {
                "key": { "item_id": { "nested": "not-a-u64" }, "tenant": "t" },
                "killer_id": { "item_id": "100", "tenant": "t" }, "victim_id": { "item_id": "200", "tenant": "t" },
                "reported_by_character_id": { "item_id": "300", "tenant": "t" }, "kill_timestamp": "0",
                "loss_type": 1, "solar_system_id": { "item_id": "0", "tenant": "t" }
            }
            """);

        var response = new ObjectsQueryData {
            Objects = new ObjectConnection {
                Nodes = [
                    new ObjectNode {
                        Address = "0xinvalid-only",
                        AsMoveObject = new MoveObjectData {
                            Contents = new MoveContents { Json = invalidKillmailJson }
                        }
                    }
                ],
                PageInfo = new PageInfo {
                    HasNextPage = true,
                    EndCursor = "cursor123"
                }
            }
        };

        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetKillmailsAsync();

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(error => error.Message.Contains("0xinvalid-only"));
    }

    [Fact]
    public async Task EmptyNodesReturnsEmptyResult() {
        var response = BuildObjectsResponse();
        _graphQlClient.QueryAsync<ObjectsQueryData>(
            Arg.Any<string>(), Arg.Any<Dictionary<string, object?>>(), Arg.Any<CancellationToken>()
        ).Returns(Result.Ok(response));

        var result = await _worldClient.GetKillmailsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Data.Should().BeEmpty();
        result.Value.HasNextPage.Should().BeFalse();
    }

    #endregion
}

