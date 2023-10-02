using JsonPathSerializer.Structs;
using JsonPathSerializer.Structs.Types.IndexSpan;
using System.Text.RegularExpressions;
using JsonPathSerializer.Structs.Path;
using JsonPathSerializer.Structs.Types.Index;

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

            if (pathTokens.Count < 1)
            {
                throw new ArgumentException("There is no valid JsonPath element in the string.");
            }

            return pathTokens;
        }

        public static List<IJsonPathToken> NewTokenize(string jsonPath)
        {
            List<IJsonPathToken> pathTokens = new();

            // Parse the JsonPath into strings of tokens.
            List<string> parsedTokenList = ParseJsonPath(jsonPath.Trim());

            foreach (string token in parsedTokenList)
            {
                // ignore root token
                if ("$.".Contains(token))
                {
                    continue;
                }

                // Try match the token into a known type.

                // match to indexes
                if (SerializerGlobals.JsonPathRegex.NEW_INDEX.IsMatch(token))
                {
                    // match the token into a collection of indexes or index spans
                    MatchCollection matches = SerializerGlobals.JsonPathRegex.NEW_INDEX_TOKEN.Matches(token);

                    JsonPathIndexToken indexToken = new();

                    foreach (Match match in matches)
                    {
                        string tokenString = match.Groups[0].Value;

                        // try matching the token to a index span
                        Match indexSpanMatch = SerializerGlobals.JsonPathRegex.NEW_INDEX_SPAN.Match(tokenString);

                        if (indexSpanMatch.Success)
                        {
                            indexToken.Add(new IndexSpanValueContainer
                            (
                                indexSpanMatch.Groups[1].Value == ""
                                    ? 0
                                    : int.Parse(indexSpanMatch.Groups[1].Value),
                                indexSpanMatch.Groups[2].Value == ""
                                    ? null
                                    : int.Parse(indexSpanMatch.Groups[2].Value)
                            ));
                        }
                        else // not a index span, thus a single index
                        {
                            indexToken.Add(new IndexValueContainer(int.Parse(tokenString)));
                        }
                    }

                    pathTokens.Add(indexToken);
                }
                else // v 0.2.0 - only option left is a property
                {
                    Match propertyMatch = SerializerGlobals.JsonPathRegex.NEW_PROPERTY.Match(token);

                    if (propertyMatch.Success)
                    {
                        pathTokens.Add(new JsonPathPropertyToken
                        (
                            propertyMatch.Groups
                                // dot notation match to Groups[1], bracket notation match to Groups[2]
                                [propertyMatch.Groups[2].Value == "" ? 1 : 2]
                                .Value
                        ));
                    }
                    else
                    {
                        throw new NotSupportedException(SerializerGlobals.ErrorMessage.UNSUPPORTED_TOKEN);
                    }
                }
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

        /// <summary>
        /// Split the JsonPath into two parts: the parent path and the leaf token.
        /// </summary>
        /// <param name="jsonPath">The complete JsonPath.</param>
        /// <returns>A tuple, the first element being the parent path to the leaf,
        /// the second being the leaf formatted into a JsonPathToken.</returns>
        public static (string, JsonPathToken) SplitPathAtLeaf(string jsonPath)
        {
            List<string> parsedTokenList = ParseJsonPath(jsonPath.Trim());

            if (parsedTokenList.Count < 1)
            {
                return ("", new JsonPathToken("", JsonPathToken.TokenType.Property));
            }

            string parentPath = string.Join("", parsedTokenList.Take(parsedTokenList.Count - 1));

            return (parentPath, Tokenize(parsedTokenList.Last())[0]);
        }
    }
}
