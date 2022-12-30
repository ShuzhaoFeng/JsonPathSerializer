using JsonPathSerializer.Structs;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Utils
{
    class JsonNodeTokenCollector
    {
        /// <summary>
        /// Navigate through a json tree to find all available nodes for a given path, and return the deepest ones.
        /// </summary>
        /// <param name="json">The json tree to scan through.</param>
        /// <param name="pathTokens">A list of JsonPathTokens indicating the path to search for.</param>
        /// <returns>A list of JsonNodeTokens containing last available node or parent of a leaf node in the json tree.</returns>
        public static List<JsonNodeToken> CollectLastAvailableTokens(JToken json, List<JsonPathToken> pathTokens)
        {
            int pathIndex = 0;
            List<JsonNodeToken> currentTokens = new() { new(json, pathIndex) };

            // Navigate through the json tree and keep track of all nodes.
            // Stop when the path is fully consumed or when all nodes are the last available.
            while (true)
            {
                if (pathIndex >= pathTokens.Count - 1) // Path fully consumed.
                {
                    return currentTokens;
                }

                JsonPathToken token = pathTokens[pathIndex];

                switch (token.Type)
                {
                    case JsonPathToken.TokenType.Property:
                        string key = (string)token.Value;

                        for (int i = 0; i < currentTokens.Count; i++)
                        {
                            JsonNodeToken currentToken = currentTokens[i];
                            JToken jToken = currentToken.Token;

                            // Check whether the token's children contains the next element of the path.
                            if (jToken is JObject currentJObject && currentJObject.ContainsKey(key))
                            {
                                currentTokens[currentTokens.IndexOf(currentToken)] =
                                    new JsonNodeToken
                                    (
                                        currentJObject[key] ?? throw new NullReferenceException(),
                                        currentToken.Index + 1
                                    );
                            }
                            else // This is the last available token.
                            {
                                currentTokens[i] = currentToken.AsLastAvailable();
                            }
                        }

                        break;

                    case JsonPathToken.TokenType.Index:
                        int index = (int)token.Value;

                        for (int i = 0; i < currentTokens.Count; i++)
                        {
                            JsonNodeToken currentToken = currentTokens[i];
                            JToken jToken = currentToken.Token;

                            // Check whether the token's children contains the next element of the path.
                            if (jToken is JArray currentJArray &&
                                currentJArray.Count > (index < 0 ? Math.Abs(index + 1) : index))
                            {
                                int absoluteIndex = index < 0 ? currentJArray.Count + index : index;
                                currentTokens[currentTokens.IndexOf(currentToken)] =
                                    new JsonNodeToken
                                    (
                                        currentJArray[absoluteIndex] ?? throw new NullReferenceException(),
                                        currentToken.Index + 1
                                    );
                            }
                            else // This is the last available token.
                            {
                                currentTokens[i] = currentToken.AsLastAvailable();
                            }
                        }

                        break;

                    case JsonPathToken.TokenType.Indexes:
                        List<int> indexes = (List<int>)token.Value;

                        int j = 0;

                        while (j < currentTokens.Count)
                        {
                            JsonNodeToken currentToken = currentTokens[j];
                            currentTokens.RemoveAt(j);
                            JToken jToken = currentToken.Token;

                            // split each token into the specified number of tokens
                            foreach (int indexOfList in indexes)
                            {
                                if (jToken is JArray currentJArray && currentJArray.Count >
                                    (indexOfList < 0 ? Math.Abs(indexOfList + 1) : indexOfList))
                                {
                                    int absoluteIndex =
                                        indexOfList < 0 ? currentJArray.Count + indexOfList : indexOfList;

                                    currentTokens.Insert(j, new JsonNodeToken
                                    (
                                        currentJArray[absoluteIndex] ?? throw new NullReferenceException(),
                                        currentToken.Index + 1
                                    ));

                                }
                                else
                                {
                                    currentTokens.Insert(j, currentToken.AsLastAvailable());
                                }

                                j++;
                            }
                        }

                        break;

                    default:
                        throw new NotImplementedException();
                }

                if (currentTokens.Any(x => !x.IsLastAvailableToken))
                {
                    pathIndex++;
                }
                else
                {
                    return currentTokens;
                }
            }
        }
    }
}
