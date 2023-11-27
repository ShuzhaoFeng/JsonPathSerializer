using JsonPathSerializer.Structs;
using JsonPathSerializer.Structs.Path;
using Newtonsoft.Json.Linq;
using static JsonPathSerializer.Globals;

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
        List<JsonNodeToken> currentTokens = new() { new JsonNodeToken(json, pathIndex) };

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
                            throw new ArgumentException
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
                    List<JsonNodeToken> newCurrentTokens = new();

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
                            JArray emptyJArray = new();
                            jToken.Replace(emptyJArray);
                            currentToken.Token = emptyJArray;

                            currentTokens[i] = currentToken.AsLastAvailable();
                        }
                        else
                        {
                            throw new ArgumentException
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
                    throw new NotSupportedException(ErrorMessage.UNSUPPORTED_TOKEN);
            }

            // if all tokens are the last available, no need to search further down.
            if (currentTokens.Any(x => !x.IsLastAvailableToken))
                pathIndex++;
            else
                return currentTokens;
        }
    }
}