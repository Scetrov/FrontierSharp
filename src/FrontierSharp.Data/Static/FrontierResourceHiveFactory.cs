using Microsoft.Extensions.Options;

namespace FrontierSharp.Data.Static;

public class FrontierResourceHiveFactory(IOptions<FrontierResourceHiveOptions> options) : IFrontierResourceHiveFactory {
    public FrontierResourceHive Create(string root) {
        return new FrontierResourceHive(root, options);
    }
}