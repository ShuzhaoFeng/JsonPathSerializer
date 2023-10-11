﻿using JsonPathSerializer.Structs;
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
                            if (jToken is JObject currentJObject && currentJObject.TryGetValue(key, out var value))
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

                    case JsonPathToken.TokenType.Index:
                        int index = (int)token.Value;

                        for (int i = 0; i < currentTokens.Count; i++)
                        {
                            JsonNodeToken currentToken = currentTokens[i];
                            JToken jToken = currentToken.Token;

                            // Check whether the token's children contains the next element of the path.
                            // if the value is positive, then wee need Count >= index + 1
                            // if the value is negative, then we need Count >= abs(index)
                            if (jToken is JArray currentJArray &&
                                currentJArray.Count >= (index < 0 ? Math.Abs(index) : index + 1))
                            {
                                // C# arrays don't accept negative indexes, so we need to convert them to positive ones.
                                // e.g. for an array of length 5, index -2 is the same as index 3, which is 5 - 2.
                                int positiveIndex = index < 0 ? currentJArray.Count + index : index;

                                // replace the current token with a new one.
                                currentTokens[currentTokens.IndexOf(currentToken)] =
                                    new JsonNodeToken
                                    (
                                        currentJArray[positiveIndex] ?? throw new NullReferenceException(),
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

                        // Indexes will multiply the number of tokens by the number of indexes if it is not the last available.
                        // for simplicity, we create a new list to store the new tokens.
                        List<JsonNodeToken> newIndexesCurrentTokens = new();

                        foreach (JsonNodeToken currentToken in currentTokens)
                        {
                            JToken jToken = currentToken.Token;

                            // split each token into the specified number of tokens
                            foreach (int indexInIndexes in indexes)
                            {
                                // Check whether the token's children contains the next element of the path.
                                // if the value is positive, then wee need Count >= index + 1
                                // if the value is negative, then we need Count >= abs(index)
                                if (jToken is JArray currentJArray && currentJArray.Count >=
                                    (indexInIndexes < 0 ? Math.Abs(indexInIndexes) : indexInIndexes + 1))
                                {
                                    // C# arrays don't accept negative indexes, so we need to convert them to positive ones.
                                    // e.g. for an array of length 5, index -2 is the same as index 3, which is 5 - 2.
                                    int positiveIndex =
                                        indexInIndexes < 0 ? currentJArray.Count + indexInIndexes : indexInIndexes;

                                    newIndexesCurrentTokens.Add(new JsonNodeToken
                                    (
                                        currentJArray[positiveIndex] ?? throw new NullReferenceException(),
                                        currentToken.Index + 1
                                    ));

                                }
                                else
                                {
                                    newIndexesCurrentTokens.Add(currentToken.AsLastAvailable());
                                }
                            }
                        }

                        currentTokens = newIndexesCurrentTokens;

                        break;

                    case JsonPathToken.TokenType.IndexSpan:
                        IndexSpanValueContainer indexSpan = (IndexSpanValueContainer)token.Value;

                        // IndexSpan will multiply the number of tokens by the number of indexes if it is not the last available.
                        // for simplicity, we create a new list to store the new tokens.
                        List<JsonNodeToken> newIndexSpanCurrentTokens = new();

                        int start = indexSpan.StartIndex;
                        int? end = indexSpan.EndIndex;

                        foreach (JsonNodeToken currentToken in currentTokens)
                        {
                            JToken jToken = currentToken.Token;

                            if (jToken is JArray currentJArray)
                            {
                                // if the end value is not provided (e.g. [1:]), then it is taken as the length of the array.
                                // start value is simpler (always 0 if not provided), so it is already handled.
                                int realEnd = end ?? currentJArray.Count;

                                int max = Math.Max(start, realEnd);
                                int min = Math.Min(start, realEnd);

                                // if the values have the same sign (e.g. [2:5]), the minimum Count required is whichever number with the greatest positive index.
                                // if the values have different signs (e.g. [-5:7]), max - min is the minimum Count required.
                                if (max > 0 && min > 0 && currentJArray.Count <= max) // both positive, same sign
                                {
                                    newIndexSpanCurrentTokens.AddRange(currentJArray.Select(
                                        t => new JsonNodeToken(
                                            t ?? throw new NullReferenceException(),
                                            currentToken.Index + 1)
                                    ));

                                    for (int i = currentJArray.Count; i <= max; i++)
                                    {
                                        newIndexSpanCurrentTokens.Add(currentToken.AsLastAvailable());
                                    }
                                }
                                else if (max > 0 && min < 0 && currentJArray.Count <= max - min) // max positive, min negative, different sign
                                {
                                    newIndexSpanCurrentTokens.AddRange(currentJArray.Select(
                                        t => new JsonNodeToken(
                                            t ?? throw new NullReferenceException(),
                                            currentToken.Index + 1)
                                    ));

                                    for (int i = currentJArray.Count; i <= max - min; i++)
                                    {
                                        newIndexSpanCurrentTokens.Add(currentToken.AsLastAvailable());
                                    }
                                }
                                else if (currentJArray.Count < - min) // both negative, same sign
                                {
                                    newIndexSpanCurrentTokens.AddRange(currentJArray.Select(
                                        t => new JsonNodeToken(
                                            t ?? throw new NullReferenceException(),
                                            currentToken.Index + 1)
                                    ));

                                    for (int i = currentJArray.Count; i < - min; i++)
                                    {
                                        newIndexSpanCurrentTokens.Add(currentToken.AsLastAvailable());
                                    }
                                }
                                else // no last available token
                                {

                                   if (start < realEnd)
                                   {
                                        for (int i = start; i <= realEnd; i += 1)
                                        {
                                            int absoluteIndex = i < 0 ? currentJArray.Count + i : i;

                                            newIndexSpanCurrentTokens.Add(new JsonNodeToken
                                            (
                                                currentJArray[absoluteIndex] ?? throw new NullReferenceException(),
                                                currentToken.Index + 1
                                            ));
                                        }
                                   }
                                   else // start > end, a reverse span
                                   {
                                        for (int i = start; i >= realEnd; i -= 1)
                                        {
                                            int absoluteIndex = i < 0 ? currentJArray.Count + i : i;

                                            newIndexSpanCurrentTokens.Add(new JsonNodeToken
                                            (
                                                currentJArray[absoluteIndex] ?? throw new NullReferenceException(),
                                                currentToken.Index + 1
                                            ));
                                        }
                                   }
                                }
                            }
                        }

                        currentTokens = newIndexSpanCurrentTokens;

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
