namespace FrontierSharp.Data.Static;

public class ResFile(string filename, string path, string hash) {
    public string Filename { get; } =
        filename.StartsWith("app:") || filename.StartsWith("res:") ? filename[4..] : filename;

    public string RelativePath { get; } = path;
    public string Hash { get; } = hash;
}