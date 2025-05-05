using System.Numerics;
using Dumpify;
using FluentAssertions;
using FrontierSharp.Mud.Linq;
using FrontierSharp.Mud.Linq.Attributes;
using Xunit;

namespace FrontierSharp.Tests.Mud.Linq;

public class MudQueryableTests {
    [Fact]
    void Test_BasicLocationQuery() {
        var provider = new MudQueryProvider();
        var location = new MudQueryable<Location>(provider);
        const string expected = "SELECT \"smartObjectId\", \"solarSystemId\", \"x\", \"y\", \"z\" FROM \"eveworld__Location\";";
        location.ToSql().Should().Be(expected);
    }
    
    [Fact]
    void Test_QueryWithOrder() {
        var provider = new MudQueryProvider();
        var location = new MudQueryable<Location>(provider);
        var query = location.OrderBy(l => l.X);
        const string expected = "SELECT \"smartObjectId\", \"solarSystemId\", \"x\", \"y\", \"z\" FROM \"eveworld__Location\" ORDER BY \"x\";";
        query.ToSql().Should().Be(expected);
    }
    
    [Fact]
    void Test_QueryWithGroupBy() {
        var provider = new MudQueryProvider();
        var location = new MudQueryable<Location>(provider);
        var query = location.GroupBy(l => l.SolarSystemId);
        const string expected = "SELECT \"solarSystemId\" FROM \"eveworld__Location\" GROUP BY \"solarSystemId\";";
        query.ToSql().Should().Be(expected);
    }
    
    [Fact]
    void Test_QueryWithDeployableState_UsingLimit() {
        var provider = new MudQueryProvider();
        var deployableState = new MudQueryable<DeployableState>(provider);

        var query = deployableState
            .Where(d => d.CurrentState == DeployableState.State.Anchored)
            .Take(10);
        const string expected = "SELECT \"smartObjectId\", \"createdAt\", \"previousState\", \"currentState\", \"isValid\", \"anchoredAt\", \"updatedBlockNumber\", \"updatedBlockTime\" FROM \"eveworld__DeployableState\" WHERE \"currentState\" = 2 LIMIT 10;";
        query.ToSql().Should().Be(expected);
    }
    
    [Fact]
    void Test_QueryWithDeployableState_UsingLimitAndOffset() {
        var provider = new MudQueryProvider();
        var deployableState = new MudQueryable<DeployableState>(provider);

        var query = deployableState
            .Where(d => d.CurrentState == DeployableState.State.Anchored)
            .Take(10)
            .Skip(0);
        const string expected = "SELECT \"smartObjectId\", \"createdAt\", \"previousState\", \"currentState\", \"isValid\", \"anchoredAt\", \"updatedBlockNumber\", \"updatedBlockTime\" FROM \"eveworld__DeployableState\" WHERE \"currentState\" = 2 LIMIT 10 OFFSET 0;";
        query.ToSql().Should().Be(expected);
    }

    [MudTable("eveworld")]
    private class DeployableState {
        [MudColumn("smartObjectId")] public BigInteger SmartObjectId { get; set; }
        [MudColumn("createdAt")] public DateTimeOffset CreatedAt { get; set; }
        [MudColumn("previousState")] public State PreviousState { get; set; }
        [MudColumn("currentState")] public State CurrentState { get; set; }
        [MudColumn("isValid")] public bool IsValid { get; set; }
        [MudColumn("anchoredAt")] public DateTimeOffset AnchoredAt { get; set; }
        [MudColumn("updatedBlockNumber")] public BigInteger UpdatedBlockNumber { get; set; }
        [MudColumn("updatedBlockTime")] public DateTimeOffset UpdatedBlockTime { get; set; }
		
        public enum State {
            Null = 0,
            Unanchored = 1,
            Anchored = 2,
            Online = 3,
            Destroyed = 4,
        }
    }

    [MudTable("eveworld", "Location")]
    private class Location {
        [MudColumn("smartObjectId")] public BigInteger SmartObjectId { get; set; }
        [MudColumn("solarSystemId")] public BigInteger SolarSystemId { get; set; }
        [MudColumn("x")] public BigInteger X { get; set; }
        [MudColumn("y")] public BigInteger Y { get; set; }
        [MudColumn("z")] public BigInteger Z { get; set; }
    }
}