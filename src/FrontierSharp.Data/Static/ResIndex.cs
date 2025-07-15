using System.IO.Abstractions;

namespace FrontierSharp.Data.Static;

public class ResIndex {
    protected ResIndex(IEnumerable<ResFile> files) {
        Files = files;
    }

    public ResIndex(string indexFile, IFileSystem fileSystem) {
        if (!fileSystem.File.Exists(indexFile)) throw new FileNotFoundException("Index file not found", indexFile);

        Files = fileSystem.File.ReadAllLines(indexFile)
            .Select(line => line.Split(","))
            .Select(fields => new ResFile(fields[0], fields[1], fields[2]))
            .AsEnumerable();
    }

    public IEnumerable<ResFile> Files { get; protected set; }

    public virtual ResFile FindByFilename(string filename) {
        Func<ResFile, bool> predicate = x => x.Filename == filename;
        if (Files.Any(predicate)) return Files.Single(predicate);

        throw new FileNotFoundException($"File not found: {filename}\n" +
                                        $"Available files: {string.Join(", ", Files.Select(x => x.Filename))}");
    }
}