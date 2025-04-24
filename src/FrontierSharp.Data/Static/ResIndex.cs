namespace FrontierSharp.Data.Static;

public class ResIndex {
    public ResIndex(string indexFile) {
        if (!File.Exists(indexFile)) throw new FileNotFoundException("Index file not found", indexFile);

        Files = File.ReadAllLines(indexFile)
            .Select(line => line.Split(","))
            .Select(fields => new ResFile(fields[0], fields[1], fields[2]))
            .AsEnumerable();
    }

    public IEnumerable<ResFile> Files { get; }

    public ResFile FindByFilename(string filename) {
        return Files.Single(x => x.Filename == filename);
    }
}