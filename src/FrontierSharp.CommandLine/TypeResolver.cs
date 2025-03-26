using Spectre.Console.Cli;

namespace FrontierSharp.CommandLine;

public class TypeResolver(IServiceProvider provider) : ITypeResolver {
    public object? Resolve(Type? type) {
        return type == null ? null : provider.GetService(type);
    }
}