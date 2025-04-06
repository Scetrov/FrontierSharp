using System.Reflection;

namespace FrontierSharp.Tests;

public static class ResourceHelper {
    public static Stream GetEmbeddedResource(string resourceName) {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) {
            throw new FileNotFoundException("Resource not found", resourceName);
        }

        return stream;
    }
}