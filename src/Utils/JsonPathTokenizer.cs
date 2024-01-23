using JsonPathSerializer.Globals;
using JsonPathSerializer.Structs.Path;
using JsonPathSerializer.Structs.Types.Index;
using JsonPathSerializer.Structs.Types.IndexSpan;
using System.Text.RegularExpressions;

namespace JsonPathSerializer.Utils;

/// <summary>
///     Collection of helper methods to convert a JsonPath string into JsonPathTokens.
/// </summary>
internal class JsonPathTokenizer
{
    /// <summary>
    ///     Split a JsonPath into a list of JsonPathTokens.
    /// </summary>
    /// <param name="jsonPath">The JsonPath to tokenize.</param>
    /// <returns>A list of JsonPathTokens from the entire JsonPath.</returns>
    public static List<IJsonPathToken> Tokenize(string jsonPath)
    {
        List<IJsonPathToken> pathTokens = new();

        // Parse the JsonPath into strings of tokens.
        List<string> parsedTokenList = ParseJsonPath(jsonPath.Trim());

        foreach (string token in parsedTokenList)
        {
            // ignore root token
            if ("$.".Contains(token)) continue;

            // Try match the token into a known type.

            // match to indexes
            if (PathRegex.Index.IsMatch(token))
            {
                // match the token into a collection of indexes or index spans
                MatchCollection matches = PathRegex.IndexToken.Matches(token);

                JsonPathIndexToken indexToken = new();

                foreach (Match match in matches)
                {
                    string tokenString = match.Groups[0].Value;

                    // try matching the token to a index span
                    Match indexSpanMatch = PathRegex.IndexSpan.Match(tokenString);

                    if (indexSpanMatch.Success)
                        indexToken.Add(new IndexSpanValueContainer
                        (
                            indexSpanMatch.Groups[1].Value == ""
                                ? 0
                                : int.Parse(indexSpanMatch.Groups[1].Value),
                            indexSpanMatch.Groups[2].Value == ""
                                ? null
                                : int.Parse(indexSpanMatch.Groups[2].Value)
                        ));
                    else // not a index span, thus a single index
                        indexToken.Add(new IndexValueContainer(int.Parse(tokenString)));
                }

                pathTokens.Add(indexToken);
            }
            else // v 0.2.0 - only option left is a property
            {
                // guard against a first property without a dot, which should be allowed.
                if (parsedTokenList.IndexOf(token) == 0 && !token.StartsWith('.') && !token.StartsWith('['))
                {
                    Match propertyDotMatch = PathRegex.Property.Match('.' + token);

                    if (propertyDotMatch.Success)
                        pathTokens.Add(new JsonPathPropertyToken
                        (
                            propertyDotMatch.Groups
                                    // dot notation match to Groups[1], bracket notation match to Groups[2]
                                    [propertyDotMatch.Groups[2].Value == "" ? 1 : 2]
                                .Value
                        ));
                }
                else
                {
                    Match propertyMatch = PathRegex.Property.Match(token);

                    if (propertyMatch.Success)
                        pathTokens.Add(new JsonPathPropertyToken
                        (
                            propertyMatch.Groups
                                    // dot notation match to Groups[1], bracket notation match to Groups[2]
                                    [propertyMatch.Groups[2].Value == "" ? 1 : 2]
                                .Value
                        ));
                    else
                        throw new NotSupportedException(ErrorMessage.UnsupportedToken);
                }
            }
        }

        return pathTokens;
    }

    /// <summary>
    ///     Split the JsonPath into two parts: the parent path and the leaf token.
    /// </summary>
    /// <param name="jsonPath">The complete JsonPath.</param>
    /// <returns>
    ///     A tuple, the first element being the parent path to the leaf,
    ///     the second being the leaf formatted into a JsonPathToken.
    /// </returns>
    public static (string, IJsonPathToken) SplitPathAtLeaf(string jsonPath)
    {
        List<string> parsedTokenList = ParseJsonPath(jsonPath.Trim());

        if (parsedTokenList.Count < 1) return ("", new JsonPathPropertyToken(""));

        string parentPath = string.Join("", parsedTokenList.Take(parsedTokenList.Count - 1));

        return (parentPath, Tokenize(parsedTokenList.Last())[0]);
    }

    /// <summary>
    ///     Parse the JsonPath into a list of string, each represents a token of the path.
    /// </summary>
    /// <param name="path">The JsonPath to tokenize.</param>
    /// <returns>A list of strings, each represents a token of the path.</returns>
    private static List<string> ParseJsonPath(string path)
    {
        List<string> paths = new();

        int index = 0;

        // Iteratively parse the string up to the last index
        while (index >= 0 && index < path.Length)
        {
            int newIndex = path[index].Equals('[')
                ? path.IndexOf(']', index) + 1 // Bracket notation
                : FindNextDotOrOpenBracket(path, index + 1); // Dot notation

            paths.Add(newIndex > 0 ? path.Substring(index, newIndex - index) : path.Substring(index));
            index = newIndex;
        }

        return paths;
    }

    /// <summary>
    ///     Locates the next dot or open bracket character in a JsonPath string.
    /// </summary>
    /// <param name="path">The JsonPath string.</param>
    /// <param name="index">Start index on the path.</param>
    /// <returns>
    ///     An integer representing the index position of the next dot or open bracket,
    ///     -1 if none found after the starting index.
    /// </returns>
    private static int FindNextDotOrOpenBracket(string path, int index)
    {
        int dotIndex = path.IndexOf('.', index);
        int bracketIndex = path.IndexOf('[', index);

        if (dotIndex == -1) return bracketIndex;

        if (bracketIndex == -1) return dotIndex;

        return Math.Min(dotIndex, bracketIndex);
    }
}