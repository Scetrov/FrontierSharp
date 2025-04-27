using System.IO.Abstractions;

namespace FrontierSharp.Data.Static;

public interface IFrontierResourceHive {
    ResIndex GetIndex(string? server = null);
    ResIndex GetResIndex(string? server = null);
    FileSystemStream GetNames();
    FileSystemStream GetTranslations();
    string ResolvePath(string relativePath);
}