using System.IO.Abstractions;
using Microsoft.Extensions.Options;

namespace FrontierSharp.Data.Static;

public class FrontierResourceHive : IFrontierResourceHive {
    private readonly FrontierResourceHiveOptions _options;
    private readonly string _resFiles;
    private readonly string _root;
    private readonly IFileSystem _fileSystem;

    public FrontierResourceHive(string root, IFileSystem fileSystem, IOptions<FrontierResourceHiveOptions> options) {
        _fileSystem = fileSystem;

        if (!_fileSystem.Directory.Exists(root)) throw new DirectoryNotFoundException($"Directory not found: {root}");

        _resFiles = _fileSystem.Path.Combine(root, "ResFiles");

        if (!_fileSystem.Directory.Exists(_resFiles)) throw new DirectoryNotFoundException($"ResFiles not found in {root}");

        _root = root;
        _options = options.Value;
    }

    public ResIndex GetIndex(string? server = null) {
        server ??= _options.ServerName;

        var path = _fileSystem.Path.Combine(_root, $"index_{server}.txt");

        if (!_fileSystem.File.Exists(path)) throw new FileNotFoundException($"Index not found: {path}", path);

        return new ResIndex(path, _fileSystem);
    }

    public ResIndex GetResIndex(string? server = null) {
        var resIndex = GetIndex(server).FindByFilename(_options.ResFileIndexKey);
        return new ResIndex(ResolvePath(resIndex.RelativePath), _fileSystem);
    }

    public FileSystemStream GetNames() {
        var localeNames = GetResIndex().FindByFilename(_options.LocalisationMainKey);
        return _fileSystem.File.OpenRead(ResolvePath(localeNames.RelativePath));
    }

    public FileSystemStream GetTranslations() {
        var englishNames = GetResIndex().FindByFilename(GetLocalisationByCulture("en-us"));
        return _fileSystem.File.OpenRead(ResolvePath(englishNames.RelativePath));
    }

    public string ResolvePath(string relativePath) {
        var path = _fileSystem.Path.Combine(_resFiles, relativePath);

        if (!_fileSystem.File.Exists(path)) throw new FileNotFoundException($"Resource file not found: {path}", path);

        return path;
    }

    private string GetLocalisationByCulture(string culture) {
        return string.Format(_options.LocalisationEnglishPatten, culture);
    }
}