using JsonPathSerializer.Structs;
using JsonPathSerializer.Structs.Path;
using JsonPathSerializer.Structs.Types;
using JsonPathSerializer.Structs.Types.Index;
using JsonPathSerializer.Structs.Types.IndexSpan;
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
        public static List<JsonNodeToken> CollectLastAvailableTokens(JToken json, List<IJsonPathToken> pathTokens)
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

                IJsonPathToken token = pathTokens[pathIndex];

                switch (token)
                {
                    case JsonPathPropertyToken propertyToken:

                        // at each level of the tree, do the same check for each token.
                        for (int i = 0; i < currentTokens.Count; i++)
                        {
                            JsonNodeToken currentToken = currentTokens[i];
                            JToken jToken = currentToken.Token;

                            
                            if (currentToken.IsLastAvailableToken) // already identified as a last available token.
                            {
                                // leave the token as it is.
                            }
                            else if // Check whether the token's children contains the next element of the path.
                            (
                                jToken is JObject currentJObject
                                && currentJObject.TryGetValue(propertyToken.Property, out JToken? value)
                            )
                            {
                                // replace the current token with a new one.
                                currentTokens[currentTokens.IndexOf(currentToken)] =
                                    new JsonNodeToken
                                    (
                                        value ?? throw new NullReferenceException(),
                                        currentToken.Index + 1
                                    );
                            }
                            else // This is the last available token.
                            {
                                currentTokens[i] = currentToken.AsLastAvailable();
                            }
                        }

                        break;

                    case JsonPathIndexToken indexToken:

                        List<JsonNodeToken> newCurrentTokens = new();

                        // at each level of the tree, do the same check for each token.
                        foreach (var currentToken in currentTokens)
                        {
                            JToken jToken = currentToken.Token;


                            if (currentToken.IsLastAvailableToken) // already identified as a last available token.
                            {
                                // leave the token as it is.
                            }
                            else if // Check whether the token's children contains the next element of the path.
                            (
                                jToken is JArray currentJArray
                                && currentJArray.Count >= indexToken.Bound
                            )
                            {
                                for (int i = 0; i < currentJArray.Count; i++)
                                {
                                    bool indexesContainsIndex = false;

                                    foreach (IValueContainer index in indexToken.Indexes)
                                    {
                                        if (index is IndexValueContainer indexValueContainer)
                                        {
                                            if (indexValueContainer.Index == i)
                                            {
                                                indexesContainsIndex = true;

                                                break;
                                            }
                                        }
                                        else if (index is IndexSpanValueContainer indexSpanValueContainer)
                                        {
                                            int start = indexSpanValueContainer.StartIndex;
                                            int end = indexSpanValueContainer.EndIndex ?? 
                                                      (start < 0 ? -1 : currentJArray.Count - 1);

                                            int min = Math.Min(start, end);
                                            int max = Math.Max(start, end);

                                            // if start >= 0 and end >= 0
                                            // then the acceptable range is [min, max]
                                            if (min >= 0)
                                            {
                                                if (i >= min && i <= max)
                                                {
                                                    indexesContainsIndex = true;

                                                    break;
                                                }
                                            }

                                            // if start < 0 and end < 0
                                            // then the acceptable range is [count + min, count + max]
                                            if (max < 0)
                                            {
                                                if (i >= currentJArray.Count + min && i <= currentJArray.Count + max)
                                                {
                                                    indexesContainsIndex = true;

                                                    break;
                                                }
                                            }

                                            // if start < 0 and end >= 0
                                            // then the acceptable range is [count + start, count - 1]
                                            // and [0, end]
                                            if (start < 0 && end >= 0)
                                            {
                                                if (i >= currentJArray.Count + start || i <= end)
                                                {
                                                    indexesContainsIndex = true;

                                                    break;
                                                }
                                            }

                                            // if start >= 0 and end < 0
                                            // then the acceptable range is [start, count + end]
                                            // or [count + end, start] depending on which one is bigger
                                            if (start >= 0 && end < 0)
                                            {
                                                if (i >= start && i <= currentJArray.Count + end)
                                                {
                                                    indexesContainsIndex = true;

                                                    break;
                                                }

                                                if (i >= currentJArray.Count + end && i <= start)
                                                {
                                                    indexesContainsIndex = true;

                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (indexesContainsIndex)
                                    {
                                        newCurrentTokens.Add(new JsonNodeToken
                                        (
                                            currentJArray[i] ?? throw new NullReferenceException(),
                                            currentToken.Index + 1
                                        ));
                                    }
                                }
                            }
                            else // This is the last available token.
                            {
                                newCurrentTokens.Add(currentToken.AsLastAvailable());
                            }
                        }

                        currentTokens = newCurrentTokens;

                        break;

                    default:
                        throw new NotSupportedException(SerializerGlobals.ErrorMessage.UNSUPPORTED_TOKEN);

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
