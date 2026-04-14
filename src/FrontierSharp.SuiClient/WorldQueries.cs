namespace FrontierSharp.SuiClient;

public static class WorldQueries {
    public const string GetObjectsByType = """
                                           query ($type: String!, $first: Int, $after: String) {
                                             objects(filter: { type: $type }, first: $first, after: $after) {
                                               nodes {
                                                 address
                                                 asMoveObject {
                                                   contents {
                                                     json
                                                     type {
                                                       repr
                                                     }
                                                   }
                                                 }
                                               }
                                               pageInfo {
                                                 hasNextPage
                                                 endCursor
                                               }
                                             }
                                           }
                                           """;

    public static string KillmailType(string packageAddress) {
        return $"{packageAddress}::killmail::Killmail";
    }

    public static string CharacterType(string packageAddress) {
        return $"{packageAddress}::character::Character";
    }

    public static string AssemblyType(string packageAddress) {
        return $"{packageAddress}::assembly::Assembly";
    }
}