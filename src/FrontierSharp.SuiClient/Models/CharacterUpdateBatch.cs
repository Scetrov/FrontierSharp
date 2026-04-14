namespace FrontierSharp.SuiClient.Models;

public class CharacterUpdateBatch {
    public bool IsInitialSnapshot { get; set; }
    public IReadOnlyList<Character> CurrentCharacters { get; set; } = [];
    public IReadOnlyList<CharacterChange> Changes { get; set; } = [];
}