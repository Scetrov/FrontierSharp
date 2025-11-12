namespace FrontierSharp.Data.Static;

public class ResFile(string filename, string path, string hash) {
    private const int PrefixLength = 4;

    public string Filename { get; } =
        filename.StartsWith("app:") || filename.StartsWith("res:") ? filename.Substring(PrefixLength, filename.Length - PrefixLength) : filename;

    public string RelativePath { get; } = path;
    public string Hash { get; } = hash;
}