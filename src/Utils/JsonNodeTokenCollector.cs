﻿using JsonPathSerializer.Exceptions;
using JsonPathSerializer.Globals;
using JsonPathSerializer.Structs;
using JsonPathSerializer.Structs.Path;
using JsonPathSerializer.Structs.Types;
using JsonPathSerializer.Structs.Types.Index;
using JsonPathSerializer.Structs.Types.IndexSpan;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Utils;

/// <summary>
///     Collection of helper methods for collecting JsonNodeTokens from the root, with the given path.
/// </summary>
internal class JsonNodeTokenCollector
{
    /// <summary>
    ///     Navigate through a json tree to find all available nodes for a given path, and return the deepest ones.
    /// </summary>
    /// <param name="json">The json tree to scan through.</param>
    /// <param name="pathTokens">A list of JsonPathTokens indicating the path to search for.</param>
    /// <param name="priority">The priority of the operation.</param>
    /// <returns>A list of JsonNodeTokens containing last available node or parent of a leaf node in the json tree.</returns>
    public static List<JsonNodeToken> CollectLastAvailableTokens(
        JToken json,
        List<IJsonPathToken> pathTokens,
        Priority priority
    )
    {
        int pathIndex = 0;
        List<JsonNodeToken> currentTokens = [new JsonNodeToken(json, pathIndex)];

        // Navigate through the json tree and keep track of all nodes.
        // Stop when the path is fully consumed or when all nodes are the last available.
        while (true)
        {
            if (pathIndex >= pathTokens.Count - 1) // Path fully consumed.
                return currentTokens;

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
                        else if (jToken is JObject currentJObject)
                        {
                            if (currentJObject.TryGetValue(propertyToken.Property, out JToken? value))
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
                        // expecting a JObject where to add property, discovered JArray or JValue instead.
                        else if (priority == Priority.High)
                        {
                            JObject emptyJObject = new();
                            jToken.Replace(emptyJObject);
                            currentToken.Token = emptyJObject;

                            currentTokens[i] = currentToken.AsLastAvailable();
                        }
                        else
                        {
                            throw new JsonPathSerializerException
                            (
                                $"JSON element $.{jToken.Path} " +
                                $"is a {jToken.Type}, therefore cannot be taken as an Object."
                            );
                        }
                    }

                    break;

                case JsonPathIndexToken indexToken:

                    // due to index tokens may increase the number of current tokens
                    // use a new list to store the new tokens to simplify code development.
                    List<JsonNodeToken> newCurrentTokens = [];

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
                            jToken is JArray currentJArray
                        )
                        {
                            if (currentJArray.Count >= indexToken.Bound)
                            {
                                for (int j = 0; j < currentJArray.Count; j++) // iterate through each index.
                                    // Check whether the index specified by the path contains the index.
                                    if (JsonValidator.ArrayContainsIndex(indexToken, j, currentJArray.Count))
                                        newCurrentTokens.Add(new JsonNodeToken
                                        (
                                            currentJArray[j] ?? throw new NullReferenceException(),
                                            currentToken.Index + 1
                                        ));
                            }
                            else // This is the last available token.
                            {
                                newCurrentTokens.Add(currentToken.AsLastAvailable());
                            }
                        }
                        // expecting a JArray where to add index, discovered JObject or JValue instead.
                        else if (priority == Priority.High)
                        {
                            JArray emptyJArray = [];
                            jToken.Replace(emptyJArray);
                            currentToken.Token = emptyJArray;

                            currentTokens[i] = currentToken.AsLastAvailable();
                        }
                        else
                        {
                            throw new JsonPathSerializerException
                            (
                                $"JSON element $.{jToken.Path} " +
                                $"is a {jToken.Type}, therefore cannot be taken as an Array."
                            );
                        }
                    }

                    // replace the current tokens with the new ones.
                    currentTokens = newCurrentTokens;

                    break;

                default:
                    throw new NotSupportedException(ErrorMessage.UnsupportedToken);
            }

            // if all tokens are the last available, no need to search further down.
            if (currentTokens.Any(x => !x.IsLastAvailableToken))
                pathIndex++;
            else
                return currentTokens;
        }
    }

    /// <summary>
    ///     Gets or creates the leaf tokens of a json tree given the leaf's parent token and the last path.
    ///     If the leaf token exists, it is returned.
    ///     If the leaf token does not exist and priority is not Low, it is created and returned.
    /// </summary>
    /// <param name="json">The leaf's parent token.</param>
    /// <param name="pathToken">The last path token.</param>
    /// <param name="priority">The priority of the operation.</param>
    /// <returns>The leaf token of the tree.</returns>
    /// <exception cref="JsonPathSerializerException">If the leaf token does not exist and priority is Low.</exception>
    public static List<JToken> GetOrCreateLeafTokens(
        JToken json,
        IJsonPathToken pathToken,
        Priority priority
    )
    {
        switch (pathToken)
        {
            case JsonPathPropertyToken propertyToken:

                if (json is JObject jObject)
                {
                    // if the path already exists, return the value.
                    if (jObject.TryGetValue(propertyToken.Property, out JToken? value))
                    {
                        return new List<JToken> { value ?? throw new Exception() };
                    }

                    // if the path doesn't exist, add a new array to the path.
                    JArray newJArray = [];

                    jObject.Add(propertyToken.Property, newJArray);

                    return new List<JToken> { newJArray };
                }

                // expecting a JObject where to add property, discovered JArray or JValue instead.
                if (priority != Priority.High)
                    throw new JsonPathSerializerException(
                        ErrorMessage.TypeConflict(nameof(JObject), json.Type.ToString())
                    );
                JObject newJObject = new()
                {
                    { propertyToken.Property, new JObject() }
                };

                json.Replace(newJObject);

                return [newJObject[propertyToken.Property] ?? throw new Exception()];

                // if not High priority, throw an exception.

            case JsonPathIndexToken indexToken:
                // if the path already exists, return the value.
                if (json is JArray jArray)
                {
                    // in case where the minimum bound is not met, add empty arrays to the path.
                    for (int i = jArray.Count; i < indexToken.Bound; i++)
                    {
                        jArray.Add(new JArray());
                    }

                    return GetTokens(jArray, indexToken);
                }

                // expecting a JArray where to add index, discovered JObject or JValue instead.
                if (priority == Priority.High)
                {
                    JArray newJArray = [];

                    for (int i = 0; i < indexToken.Bound; i++)
                    {
                        newJArray.Add(new JArray());
                    }

                    json.Replace(newJArray);

                    return GetTokens(newJArray, indexToken);
                }

                // if not High priority, throw an exception.
                throw new JsonPathSerializerException(
                    ErrorMessage.TypeConflict("JArray", json.Type.ToString())
                );

            default:
                throw new NotSupportedException(ErrorMessage.UnsupportedToken);
        }
    }

    private static List<JToken> GetTokens(JArray jArray, JsonPathIndexToken indexToken)
    {
        List<JToken> tokens = [];

        foreach (IValueContainer container in indexToken.Indexes)
            // replace the element at the specified index/index span with the new value.

            if (container is IndexValueContainer indexValueContainer)
            {
                int index = indexValueContainer.Index;

                // if the index is negative, count the index from the end of the array.
                tokens.Add(jArray[index >= 0 ? index : jArray.Count + index]);
            }
            else if (container is IndexSpanValueContainer indexSpanValueContainer)
            {
                int start = indexSpanValueContainer.StartIndex;

                // if the end index is not specified, it is defaulted the last index of the array.
                int end = indexSpanValueContainer.EndIndex ??
                          (start < 0 ? -1 : jArray.Count - 1);

                int min = Math.Min(start, end);
                int max = Math.Max(start, end);

                // replace the value across the span.
                for (int i = min; i <= max; i++) tokens.Add(jArray[i >= 0 ? i : jArray.Count + i]);
            }

        return tokens;
    }
}