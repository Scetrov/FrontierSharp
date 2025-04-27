using System.IO.Abstractions;
using Microsoft.Extensions.Options;

namespace FrontierSharp.Data.Static;

public class FrontierResourceHiveFactory(IFileSystem fileSystem, IOptions<FrontierResourceHiveOptions> options) : IFrontierResourceHiveFactory {
    public IFrontierResourceHive Create(string root) {
        return new FrontierResourceHive(root, fileSystem, options);
    }
}