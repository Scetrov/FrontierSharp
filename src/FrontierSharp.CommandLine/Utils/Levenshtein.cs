namespace FrontierSharp.CommandLine.Utils;

public static class Levenshtein {
    public static int Distance(string source, string target) {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        var sourceLength = source.Length;
        var targetLength = target.Length;
        var previousRow = new int[targetLength + 1];
        var currentRow = new int[targetLength + 1];

        for (var j = 0; j <= targetLength; j++) previousRow[j] = j;

        for (var i = 1; i <= sourceLength; i++) {
            currentRow[0] = i;
            var sourceChar = source[i - 1];

            for (var j = 1; j <= targetLength; j++) {
                var cost = sourceChar == target[j - 1] ? 0 : 1;
                currentRow[j] = Math.Min(Math.Min(currentRow[j - 1] + 1, previousRow[j] + 1), previousRow[j - 1] + cost);
            }

            Array.Copy(currentRow, previousRow, targetLength + 1);
        }

        return previousRow[targetLength];
    }
}