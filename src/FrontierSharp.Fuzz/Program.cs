using System.Text;
using System.Text.Json;
using Razorvine.Pickle;
using SharpFuzz;
using System.IO.Abstractions;
using FrontierSharp.Data.Static;
using FrontierSharp.SuiClient.GraphQl;
using FrontierSharp.SuiClient.JsonConverters;
using FrontierSharp.SuiClient.Models;
using FrontierSharp.SuiClient;
using FrontierSharp.HttpClient;
using FrontierSharp.HttpClient.Models;
using FrontierSharp.WorldApi;
using FrontierSharp.WorldApi.Models;
using FrontierSharp.WorldApi.RequestModel;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

var targets = new Dictionary<string, Action<Stream>>(StringComparer.OrdinalIgnoreCase) {
    ["sui"] = FuzzTargets.Sui,
    ["resindex"] = FuzzTargets.ResIndex,
    ["pickle"] = FuzzTargets.Pickle,
    ["world"] = FuzzTargets.World
};

if (args.Length < 2 || !targets.TryGetValue(args[1], out var target)) {
    Console.Error.WriteLine("Usage: FrontierSharp.Fuzz <replay|fuzz> <sui|resindex|pickle|world> [corpus-directory]");
    return 2;
}

if (string.Equals(args[0], "fuzz", StringComparison.OrdinalIgnoreCase)) {
    Fuzzer.Run(stream => FuzzTargets.Run(target, stream));
    return 0;
}

if (!string.Equals(args[0], "replay", StringComparison.OrdinalIgnoreCase) || args.Length != 3) return 2;
foreach (var seed in Directory.EnumerateFiles(args[2]).OrderBy(path => path, StringComparer.Ordinal)) {
    using var input = File.OpenRead(seed);
    FuzzTargets.Run(target, input);
    Console.WriteLine(Path.GetFileName(seed));
}
return 0;

internal sealed class FuzzHttpClientFactory(string payload) : IHttpClientFactory {
    public HttpClient CreateClient(string name) => new(new FuzzHandler(payload));
    private sealed class FuzzHandler(string payload) : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(payload, Encoding.UTF8, "application/json") });
    }
}

internal sealed class FuzzHybridCache : HybridCache {
    public override ValueTask<T> GetOrCreateAsync<TState, T>(string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default) => factory(state, cancellationToken);
    public override ValueTask SetAsync<T>(string key, T value, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}

internal static class FuzzTargets {
    public static void Run(Action<Stream> target, Stream input) {
        try { target(input); }
        catch (JsonException) { }
        catch (FormatException) { }
        catch (ArgumentException) { }
        catch (IndexOutOfRangeException) { }
        catch (FileNotFoundException) { }
        catch (InvalidOperationException) { }
        catch (Razorvine.Pickle.InvalidOpcodeException) { }
        catch (Razorvine.Pickle.PickleException) { }
        catch (NotSupportedException) { }
        catch (IOException) { }
    }

    public static void Sui(Stream input) {
        using var payload = new MemoryStream();
        input.CopyTo(payload);
        var bytes = payload.ToArray();
        var body = Encoding.UTF8.GetString(bytes);
        var client = new SuiGraphQlClient(new FuzzHttpClientFactory(body), new FuzzHybridCache(),
            Microsoft.Extensions.Options.Options.Create(new SuiClientOptions { HttpClientName = "Fuzz", GraphQlEndpoint = "https://fuzz.invalid/graphql" }),
            NullLogger<SuiGraphQlClient>.Instance);
        _ = client.QueryAsync<ObjectsQueryData>("query { objects { nodes { address } } }").GetAwaiter().GetResult();
        var options = new JsonSerializerOptions();
        options.Converters.Add(new CharacterMetadataConverter());
        _ = JsonSerializer.Deserialize<CharacterMetadata>(bytes, options);
    }

    public static void ResIndex(Stream input) {
        var root = Path.Combine(Path.GetTempPath(), "frontiersharp-fuzz", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try {
            var path = Path.Combine(root, "index.csv");
            File.WriteAllText(path, new StreamReader(input, Encoding.UTF8, true, 1_048_576, leaveOpen: true).ReadToEnd());
            _ = new ResIndex(path, new FileSystem()).Files.ToArray();
        } finally { Directory.Delete(root, recursive: true); }
    }

    public static void Pickle(Stream input) {
        using var unpickler = new Unpickler();
        _ = unpickler.load(input);
    }

    public static void World(Stream input) {
        using var memory = new MemoryStream();
        input.CopyTo(memory);
        memory.Position = 0;
        var bytes = memory.ToArray();
        var client = new FrontierSharpHttpClient(
            NullLogger<FrontierSharpHttpClient>.Instance,
            new FuzzHttpClientFactory(Encoding.UTF8.GetString(bytes)),
            new FuzzHybridCache(),
            Microsoft.Extensions.Options.Options.Create(new FrontierSharpHttpClientOptions {
                HttpClientName = "Fuzz",
                BaseUri = "https://fuzz.invalid/"
            }));
        _ = client.Get<GetListOfTypes, WorldApiPayload<GameType>>(new GetListOfTypes()).GetAwaiter().GetResult();
    }
}
