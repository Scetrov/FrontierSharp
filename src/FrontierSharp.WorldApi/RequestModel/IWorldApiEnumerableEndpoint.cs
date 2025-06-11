namespace FrontierSharp.WorldApi.RequestModel;

public interface IWorldApiEnumerableEndpoint {
    public long Limit { get; set; }
    public long Offset { get; set; }
}