using JsonPathSerializer.Structs;
using JsonPathSerializer.Structs.Types.IndexSpan;
using System.Text.RegularExpressions;

namespace JsonPathSerializer.Utils
{
    class JsonPathTokenizer
    {
        /// <summary>
        /// Split a JsonPath into a list of JsonPathTokens.
        /// </summary>
        /// <param name="jsonPath">The JsonPath to tokenize.</param>
        /// <returns>A list of JsonPathTokens from the entire JsonPath.</returns>
        public static List<JsonPathToken> Tokenize(string jsonPath)
        {
            List<JsonPathToken> pathTokens = new();

            // Parse the JsonPath into strings of tokens.
            List<string> parsedTokenList = ParseJsonPath(jsonPath.Trim());


            foreach (string pathToken in parsedTokenList)
            {
                // Ignore root token
                if ("$.".Contains(pathToken))
                {
                    continue;
                }

                // Try match the token into a known type.

                Match indexSpanMatch = SerializerGlobals.JsonPathRegex.INDEX_SPAN.Match(pathToken);

                if (indexSpanMatch.Success)
                {

                    pathTokens.Add(new JsonPathToken(
                        new IndexSpanValueContainer
                        (
                            indexSpanMatch.Groups[1].Value == ""
                                ? 0
                                : int.Parse(indexSpanMatch.Groups[1].Value),
                            indexSpanMatch.Groups[2].Value == ""
                                ? null
                                : int.Parse(indexSpanMatch.Groups[2].Value)
                        ),
                        JsonPathToken.TokenType.IndexSpan
                    ));

                    continue;
                }

                Match indexListMatch = SerializerGlobals.JsonPathRegex.INDEXES.Match(pathToken);

                if (indexListMatch.Success)
                {
                    pathTokens.Add(new JsonPathToken(
                        new Regex(@"-?\d+").Matches(indexListMatch.Value)
                            .Select(index => int.Parse(index.Value)).ToList(),
                        JsonPathToken.TokenType.Indexes
                    ));

                    continue;
                }

                Match indexMatch = SerializerGlobals.JsonPathRegex.INDEX.Match(pathToken);

                if (indexMatch.Success)
                {
                    pathTokens.Add(new JsonPathToken(int.Parse(indexMatch.Groups[1].Value),
                        JsonPathToken.TokenType.Index));
                    continue;
                }

                Match propertyBracketMatch = SerializerGlobals.JsonPathRegex.PROPERTY_BRACKET.Match(pathToken);

                if (propertyBracketMatch.Success)
                {
                    pathTokens.Add(new JsonPathToken(propertyBracketMatch.Groups[1].Value,
                        JsonPathToken.TokenType.Property));
                    continue;
                }

                // guard against a first property without a dot, which should be allowed.
                if (parsedTokenList.IndexOf(pathToken) == 0 && !pathToken.StartsWith('.'))
                {
                    Match propertyDotMatch = SerializerGlobals.JsonPathRegex.PROPERTY_DOT.Match('.' + pathToken);

                    if (propertyDotMatch.Success)
                    {
                        pathTokens.Add(
                            new JsonPathToken(propertyDotMatch.Groups[1].Value, JsonPathToken.TokenType.Property));
                        continue;
                    }
                }
                else
                {
                    Match propertyDotMatch = SerializerGlobals.JsonPathRegex.PROPERTY_DOT.Match(pathToken);

                    if (propertyDotMatch.Success)
                    {
                        pathTokens.Add(
                            new JsonPathToken(propertyDotMatch.Groups[1].Value, JsonPathToken.TokenType.Property));
                        continue;
                    }
                }

                // If any token is not a known type, stop the process.
                throw new NotSupportedException(SerializerGlobals.ErrorMessage.UNSUPPORTED_TOKEN);
            }

            return pathTokens;
        }

        /// <summary>
        /// Parse the JsonPath into a list of string, each represents a token of the path.
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
        /// Locates the next dot or open bracket character in a JsonPath string.
        /// </summary>
        /// <param name="path">The JsonPath string.</param>
        /// <param name="index">Start index on the path.</param>
        /// <returns>
        /// An integer representing the index position of the next dot or open bracket,
        /// -1 if none found after the starting index.
        /// </returns>
        private static int FindNextDotOrOpenBracket(string path, int index)
        {
            int dotIndex = path.IndexOf('.', index);
            int bracketIndex = path.IndexOf('[', index);

            if (dotIndex == -1)
            {
                return bracketIndex;
            }

            if (bracketIndex == -1)
            {
                return dotIndex;
            }

            return Math.Min(dotIndex, bracketIndex);
        }
    }
}
